using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;


namespace Lincsolution.Core.Communication.OpcUa
{
    public class OpcClient : IOpcClient
    {
        #region Variables
        /// <summary>(필요 시)Client 접속용 Username</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>(필요 시)Client 접속용 Password</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>마지막 Exception 데이터</summary>
        public OpcClientException LastException { get; private set; }

        /// <summary>Opc Ua 세션</summary>
        private ISession Session { get; set; }

        /// <summary>Console 또는 EventLog 기능 사용 가능한 컨택스트 클래스</summary>
        private readonly ITelemetryContext telemetry;

        /// <summary>MonitoredItems로 포함한 노드를 Session에서 구독하고 모니터링하는 클래스</summary>
        private Subscription subscription;

        /// <summary>구독할 노드 리스트</summary>
        private List<MonitoredItem> monitoredItems;

        /// <summary>Opc Ua 인증서 관리 폴더 경로</summary>
        private string certificatePath = string.Empty;

        /// <summary>Opc Ua 설정 정보</summary>
        private ApplicationConfiguration appConfig;

        /// <summary>Session 재연결 핸들러</summary>
        private SessionReconnectHandler reconnectHandler;

        /// <summary>Session 재연결 여부 저장</summary>
        private int isReconnectInProgress;

        /// <summary>Opc Ua Session의 Subscription으로 자동 변경할 대상 인스턴스</summary>
        private object targetInstance = new object();

        /// <summary>targetInstance의 Property이름, Property정보로 구성된 사전</summary>
        private Dictionary<string, PropertyInfo> propertyMap;

        /// <summary>Session 접속 URL</summary>
        private string sessionUrl = string.Empty;

        /// <summary>Session 이름</summary>
        private string sessionName = string.Empty;
        
        /// <summary>Subscription 이름</summary>
        private string subscriptionName = string.Empty;

        /// <summary>Session의 응답 없음 대기 시간 Timeout(ms)</summary>
        private uint sessionTimeout = 0;

        /// <summary>Subscription 갱신 주기(ms)</summary>
        private int subscriptionInterval = 0;

        /// <summary>
        /// Subscription 갱신 항목 없음으로 서버 대기중일 때, KeepAlive 메세지 전송 기준값
        /// 실제 KeepAlive 메세지 전송 시간은 publishingInterval(ms) * subscriptionKeepAliveCount
        /// </summary>
        private uint subscriptionKeepAliveCount = 0;

        /// <summary>
        /// Client 응답 없을 때, 상대 Server가 Subscription 폐기하기까지 기다릴 Publish 횟수
        /// 실제 폐기 시간은 PublishingInterval(ms) * subscriptionLifetimeCount
        /// 이 값은 항상 subscriptionKeepAliveCount 보다 커야 함.
        /// </summary>
        private uint subscriptionLifetimeCount = 0;

        /// <summary>Subscription의 1회 갱신 당 최대 몇 개의 노드 변경 알림을 받을 건지</summary>
        private uint subscriptionMaxNotificationPerPublish = 0;
        #endregion


        public OpcClient()
        {
            telemetry = DefaultTelemetry.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddDebug();
            });
        }

        /// <summary>
        /// OpcClient 클래스 생성자. 연결 URL, 세션명, 클래스타입 입력
        /// </summary>
        /// <param name="url">실제로 연결할 opc 서버 URL</param>
        /// <param name="name">세션 구분용 명칭</param>
        /// <param name="certificatePath">인증서 저장 위치</param>
        /// <param name="targetInstance">OpcClient Subscription 갱신시마다 연동해서 값 갱신해줄 변수 모음 클래스</param>
        /// <param name="monitoredItemsList">Subscription 으로 모니터링할 값 리스트</param>
        /// <param name="traceLogPath">OpcClient 로그 저장 경로</param>
        /// <param name="sessionTimeout">세션 연결 Timeout 시간(ms)</param>
        /// <param name="subscriptionInterval">Subscription 갱신 시간(ms)</param>
        /// <param name="subscriptionMaxNotificationPerPublish">Subscription 1회 갱신 시 최대로 확인할 노드 갯수</param>
        public bool Connect(string url, string name, string certPath,
            object targetInstance, IEnumerable<OpcSubscriptionItem> subscriptionItemList,
            uint sessionTimeout = 60_000, int subscriptionInterval = 50, uint subscriptionMaxNotificationPerPublish = 300,
            bool useIdentity = false, ushort keySize = 1024)
        {
            try
            {
                LastException = null;
                sessionUrl = url;
                sessionName = name;
                certificatePath = certPath;
                monitoredItems = new List<MonitoredItem>();

                appConfig = CreateOpcUaAppConfiguration(name, keySize);
                if(appConfig == null)
                {
                    throw new OpcClientException(OpcClientExceptionType.CertificateCreateFailed, "Failed to Create Certificate");
                }

                this.targetInstance = targetInstance;
                propertyMap = targetInstance.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
                subscriptionName = "Subscription" + $"_{sessionName}_" + url;
                
                this.sessionTimeout = sessionTimeout;
                this.subscriptionInterval = subscriptionInterval;
                subscriptionKeepAliveCount = 20;
                subscriptionLifetimeCount = subscriptionKeepAliveCount * 3;
                this.subscriptionMaxNotificationPerPublish = subscriptionMaxNotificationPerPublish;

                /// 세션 생성
                var sessionFactory = new DefaultSessionFactory(telemetry);
                /// UserName/Password 있는지 판별해서 연결
                if(useIdentity)
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(Password ?? string.Empty);
                    IUserIdentity userIdentity = new UserIdentity(UserName, passwordBytes);
                    Session = sessionFactory.CreateAsync(appConfig, new ConfiguredEndpoint(null, new EndpointDescription(sessionUrl)), true,
                        sessionName, sessionTimeout, userIdentity, null).GetAwaiter().GetResult();
                }
                else
                {
                    Session = sessionFactory.CreateAsync(appConfig, new ConfiguredEndpoint(null, new EndpointDescription(sessionUrl)), true,
                        sessionName, sessionTimeout, null, null).GetAwaiter().GetResult();
                }

                /// Session = null 또는 Connected 아닐 때의 예외 처리
                if(Session is null)
                {
                    throw new OpcClientException(OpcClientExceptionType.isNullSession);
                }
                else if(!Session.Connected)
                {
                    throw new OpcClientException(OpcClientExceptionType.SessionNotConnected);
                }
                else
                {
                    /// 세션 연결 끊겼을 때, 자동 연결 이벤트 함수 연결
                    Session.KeepAlive += SessionKeepAlive;
                    /// 재연결 시작 시 Subscription 전달 여부 설정
                    Session.TransferSubscriptionsOnReconnect = true;
                    /// 재연결 클래스 생성
                    reconnectHandler = new SessionReconnectHandler(Session.MessageContext.Telemetry, reconnectAbort: true, maxReconnectPeriod: -1);
                    /// MonitoredItems에 subscriptionItemList 추가
                    foreach(var item in subscriptionItemList)
                    {
                        monitoredItems.Add(new MonitoredItem(Session.MessageContext.Telemetry)
                        {
                            DisplayName = item.DisplayName,
                            StartNodeId = item.StartNodeId
                        });
                    }
                }
            }
            catch(OpcClientException ex)
            {
                LastException = ex;
                return false;
            }
            catch(Exception ex)
            {
                LastException = new OpcClientException(OpcClientExceptionType.UnknownException, "OPC UA connection failed", ex);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// OPC UA 세션 연결 및 관련 데이터 해제 함수
        /// Session, reconnectHandler, subscription, monitoredItems, propertyMap 전부 해제.
        /// </summary>
        public void DisConnect()
        {
            ISession session = Session;

            try
            {
                if(session is null)
                {
                    throw new OpcClientException(OpcClientExceptionType.isNullSession, "Disconnect failed by null Session");
                }
                else
                {
                    if(reconnectHandler is null)
                    {
                        throw new OpcClientException(OpcClientExceptionType.isNullReconnectHandler, "Disconnect failed by null ReconnectHandler");
                    }
                    else
                    {
                        session.KeepAlive -= SessionKeepAlive;
                        reconnectHandler.Dispose();
                        reconnectHandler = null;
                    }

                    if(session.Connected)
                    {
                        session.CloseAsync().Wait();
                    }
                    Debug.WriteLine($"OPC UA Session[{sessionName}] Closed");
                }
            }
            catch(OpcClientException ex)
            {
                LastException = ex;
            }
            catch(Exception ex)
            {
                LastException = new OpcClientException(OpcClientExceptionType.UnknownException, ex);
            }
            finally
            {
                Utils.SilentDispose(session);
                Session = null;
                reconnectHandler = null;
                Interlocked.Exchange(ref isReconnectInProgress, 0);

                if(!(monitoredItems is null))
                {
                    monitoredItems.ForEach(i => i.Notification -= new MonitoredItemNotificationEventHandler(OnMonitoredItemChanged));
                }
                Utils.SilentDispose(subscription);
                subscription = null;
                monitoredItems = null;
                propertyMap = null;
            }
        }

        /// <summary>
        /// Session의 MonitoredItems로 Subscription 생성하는 함수
        /// </summary>
        /// <exception cref="OpcClientException"></exception>
        public void CreateSubscription()
        {
            Subscription subscription = null;
            bool handlersAttached = false;

            try
            {
                LastException = null;
                if(Session is null)
                {
                    throw new OpcClientException(OpcClientExceptionType.isNullSession, "OpcError_IsNullSession");
                }
                if(!Session.Connected)
                {
                    throw new OpcClientException(OpcClientExceptionType.SessionNotConnected, "OpcError_NotConnected");
                }
                if(monitoredItems is null || monitoredItems.Count == 0)
                {
                    throw new OpcClientException(OpcClientExceptionType.SubscriptionCreateFailed, "No monitored items configured.");
                }

                subscription = new Subscription(Session.DefaultSubscription);
                subscription.DisplayName = subscriptionName;
                subscription.PublishingEnabled = true;
                subscription.PublishingInterval = subscriptionInterval;
                subscription.KeepAliveCount = subscriptionKeepAliveCount;
                subscription.LifetimeCount = subscriptionLifetimeCount;
                subscription.MaxNotificationsPerPublish = subscriptionMaxNotificationPerPublish;
                monitoredItems.ForEach(i => i.Notification += new MonitoredItemNotificationEventHandler(OnMonitoredItemChanged));
                handlersAttached = true;
                subscription.AddItems(monitoredItems);
                Session.AddSubscription(subscription);
                subscription.CreateAsync().GetAwaiter().GetResult();

                if(!subscription.Created)
                {
                    throw new OpcClientException(OpcClientExceptionType.SubscriptionCreateFailed, "OPC UA subscription was not created.");
                }

                this.subscription = subscription;
            }
            catch(OpcClientException ex)
            {
                LastException = ex;
                if(handlersAttached && monitoredItems != null)
                {
                    monitoredItems.ForEach(i => i.Notification -= new MonitoredItemNotificationEventHandler(OnMonitoredItemChanged));
                }
                Utils.SilentDispose(subscription);
                throw;
            }
            catch(Exception ex)
            {
                if(handlersAttached && monitoredItems != null)
                {
                    monitoredItems.ForEach(i => i.Notification -= new MonitoredItemNotificationEventHandler(OnMonitoredItemChanged));
                }
                Utils.SilentDispose(subscription);
                throw new OpcClientException(OpcClientExceptionType.SubscriptionCreateFailed, "Failed to create OPC UA subscription.", ex);
            }
        }

        /// <summary>
        /// Opc Ua Server Node에 데이터 전송
        /// </summary>
        /// <param name="id">전송할 노드 ID</param>
        /// <param name="data">전송할 데이터</param>
        public bool Write(NodeId id, object data)
        {
            try
            {
                LastException = null;
                /// 세션 없거나 미연결 시 예외 처리
                if(Session is null)
                {
                    throw new OpcClientException(OpcClientExceptionType.isNullSession, "OPC UA Write failed by null Session");
                }
                if(!Session.Connected)
                {
                    throw new OpcClientException(OpcClientExceptionType.SessionNotConnected, "OPC UA Write failed by Session not Connected");
                }
                /// Write Instance 생성 후 Node ID, Attribute ID 설정
                WriteValue writeValue = new WriteValue();
                writeValue.NodeId = id;
                writeValue.AttributeId = Attributes.Value;
                /// Write 할 Node Type 획득
                Type nodeType = GetNodeDataType(id);
                /// 전송 타입에 맞게 변환 처리
                object convertedValue = ConvertValueByNodeType(nodeType, data);
                
                /// 변환 값 null 검사 후 writeValue.Value.Value 에 변환값 할당
                if(convertedValue is null)
                {
                    throw new OpcClientException(OpcClientExceptionType.WrongData, "OPC UA Write failed by wrong data");
                }
                else
                {
                    writeValue.Value.Value = convertedValue;
                }

                /// 전송 데이터 Collection 생성 후 writeValue 할당.
                WriteValueCollection valuesToWrite = new WriteValueCollection();
                valuesToWrite.Add(writeValue);
                /// Session 기본 동작 Async라, 취소 토큰 추가. Write 즉시 응답 생성
                CancellationToken ct = new CancellationToken();
                WriteResponse response = Session.WriteAsync(null, valuesToWrite, ct).GetAwaiter().GetResult();
                /// 전송결과 및 진단 결과 확인
                StatusCodeCollection sendResults = response.Results;
                DiagnosticInfoCollection diagsInfos = response.DiagnosticInfos;
                /// 돌아온 response 데이터 분류
                ClientBase.ValidateResponse(sendResults, valuesToWrite);
                ClientBase.ValidateDiagnosticInfos(diagsInfos, valuesToWrite);
                /// 전송 결과가 IsBad인 경우, ServiceResultException으로 처리
                if(StatusCode.IsBad(sendResults[0]))
                {
                    throw ServiceResultException.Create(sendResults[0], 0, diagsInfos, response.ResponseHeader.StringTable);
                }
                return true;
            }
            catch(ServiceResultException ex)
            {
                LastException = new OpcClientException(OpcClientExceptionType.ServiceResultException, ex.Message, ex);
                return false;
            }
            catch(OpcClientException ex)
            {
                LastException = ex;
                return false;
            }
            catch(Exception ex)
            {
                LastException = new OpcClientException(OpcClientExceptionType.UnknownException, "OPC UA Write failed", ex);
                return false;
            }
            finally
            {
                /// Write 종료 후 ReturnDiagnostics 값 None으로 초기화
                if(Session != null)
                {
                    Session.ReturnDiagnostics = DiagnosticsMasks.None;
                }
            }
        }

        /// <summary>
        /// 이름으로 Opc Ua Server 노드 아이디 찾는 함수
        /// </summary>
        /// <param name="name">찾을 아이디 이름</param>
        /// <returns>노드 아이디</returns>
        public NodeId GetNodeId(string name)
        {
            if(monitoredItems == null)
            {
                throw new OpcClientException(OpcClientExceptionType.MonitoredItemNotFound, "Get Node ID failed");
            }

            int index = monitoredItems.FindIndex(x => x.DisplayName == name);
            if(index < 0)
            {
                throw new OpcClientException(OpcClientExceptionType.MonitoredItemNotFound, $"Monitored item not found: {name}");
            }

            return monitoredItems[index].StartNodeId;
        }

        private ApplicationConfiguration CreateOpcUaAppConfiguration(string appName, ushort keySize)
        {
            try
            {
                /// Security Configuration 인증서 경로 지정
                var certPathBase = Path.Combine(certificatePath, "OPC Foundation\\CertificateStores\\");
                var trustedIssuerPath = Path.Combine(certPathBase, "UA Certificate Authorities");
                var trustedPeerPath = Path.Combine(certPathBase, "UA Applications");
                var rejectedPath = Path.Combine(certPathBase, "Rejected Certificates");
                var ownPath = Path.Combine(certPathBase, "Own");

                /// Security Configuration 인증서 경로 생성
                Directory.CreateDirectory(certPathBase);
                Directory.CreateDirectory(trustedIssuerPath);
                Directory.CreateDirectory(trustedPeerPath);
                Directory.CreateDirectory(rejectedPath);
                Directory.CreateDirectory(ownPath);

                var configuration = new ApplicationConfiguration(telemetry)
                {
                    ApplicationName = appName,
                    ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:{appName}",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = "Directory",
                            StorePath = ownPath,
                            SubjectName = $"CN={appName}",
                            CertificateType = ObjectTypeIds.RsaSha256ApplicationCertificateType
                        },
                        TrustedIssuerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = trustedIssuerPath
                        },
                        TrustedPeerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = trustedPeerPath
                        },
                        RejectedCertificateStore = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = rejectedPath
                        },

                        MinimumCertificateKeySize = keySize,
                        CertificatePasswordProvider = new CertificatePasswordProvider(Array.Empty<char>()),
                        // 장비 최초 연결/provisioning 편의 때문에 untrusted cert 는 자동 신뢰 등록한다.
                        // 단, 아래 validation handler 에서 RSA key size 정책은 먼저 강제한다.
                        AutoAcceptUntrustedCertificates = true,
                        AddAppCertToTrustedStore = true,
                        RejectSHA1SignedCertificates = false
                    },
                    TransportQuotas = new TransportQuotas {OperationTimeout = (int)sessionTimeout},
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = (int)sessionTimeout},                    
                };

                var appInstance = new ApplicationInstance(configuration, telemetry)
                {
                    ApplicationName = appName,
                    ApplicationType = ApplicationType.Client,
                };

                configuration.ValidateAsync(ApplicationType.Client).GetAwaiter().GetResult();
                appInstance.CheckApplicationInstanceCertificatesAsync(false).GetAwaiter().GetResult();
                if(configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    OpcUtility.AttachCertificateValidationHandler(configuration, telemetry, keySize);
                }
                return configuration;
            }
            catch(ServiceResultException sre)
            {
                throw new OpcClientException(OpcClientExceptionType.CertificateCreateFailed, "Certificate Creation Failed", sre);
            }
            catch(Exception ex) when(!(ex is OpcClientException))
            {
                throw new OpcClientException(OpcClientExceptionType.UnknownException, "Unknown Exception", ex);
            }
        }

        /// <summary>
        /// 세션 계속 연결상태로 유지하기 위한 이벤트 상태 나쁠 때, 자동으로 1초간 재연결
        /// </summary>
        /// <param name="session">세션 인터페이스</param>
        /// <param name="e"></param>
        private void SessionKeepAlive(ISession session, KeepAliveEventArgs e)
        {
            if(!ServiceResult.IsBad(e.Status))
            {
                return;
            }

            if(reconnectHandler == null || Session == null)
            {
                Debug.WriteLine($"Session[{sessionName}] Status bad but reconnect handler/session is not ready.");
                return;
            }

            if(Interlocked.CompareExchange(ref isReconnectInProgress, 1, 0) != 0)
            {
                Debug.WriteLine($"Session[{sessionName}] Status bad while reconnect is already in progress.");
                return;
            }

            try
            {
                /// TODO : 연결 끊겼을 때 알람이나 무언가 처리할거면 하기.=> 근데 해보니까 알람 없이 로그만 남기는게 나았음.
                Debug.WriteLine($"Session[{sessionName}] Status bad. Start Reconnection");
                reconnectHandler.BeginReconnect(session, 1000, ServerReconnectCompleted);
            }
            catch(Exception ex)
            {
                Interlocked.Exchange(ref isReconnectInProgress, 0);
                LastException = new OpcClientException(OpcClientExceptionType.ReconnectFailed, ex);
            }
        }

        /// <summary>
        /// 세션 서버 재연결 성공 시 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerReconnectCompleted(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine($"Session[{sessionName} Reconnect Completed]");
                var reconnectHandler = this.reconnectHandler;
                if(reconnectHandler?.Session == null)
                {
                    return;
                }

                if(!ReferenceEquals(Session, reconnectHandler.Session))
                {
                    var oldSession = Session;
                    if(oldSession != null)
                    {
                        oldSession.KeepAlive -= SessionKeepAlive;
                    }

                    Session = reconnectHandler.Session;
                    Session.KeepAlive += SessionKeepAlive;
                    Utils.SilentDispose(oldSession);
                    Debug.WriteLine("Recreate Session Complete");
                    /// TODO : 여기도 로그 남기거나 하기
                }
            }
            catch(Exception ex)
            {
                LastException = new OpcClientException(OpcClientExceptionType.ReconnectFailed, ex);
            }
            finally
            {
                Interlocked.Exchange(ref isReconnectInProgress, 0);
            }
        }

        /// <summary>
        /// MonitoredItem에 등록한 각 item의 DisplayName 과 
        /// _targetInstance의 PropertyInfo로 구성한 _propertyMap Dictionary의 key값과 비교하고,
        /// 일치하면 _targetInstance의 Property 값을 SetValue 로 갱신
        /// </summary>
        /// <param name="item">Subscription 으로 갱신조회할 아이템들</param>
        /// <param name="e">갱신 시 발생하는 이벤트</param>
        private void OnMonitoredItemChanged(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                var session = Session;
                var subscription = this.subscription;
                var propertyMap = this.propertyMap;
                var targetInstance = this.targetInstance;
                

                if(session is null || !session.Connected || subscription is null || propertyMap is null || targetInstance is null)
                {
                    Debug.WriteLine($"[OPC UA] Skip monitored notification. Client state is not ready. Item={item?.DisplayName}");
                    return;
                }

                if(item == null || string.IsNullOrWhiteSpace(item.DisplayName))
                {
                    Debug.WriteLine("[OPC UA] Skip monitored notification. Monitored item is invalid.");
                    return;
                }

                foreach(var value in item.DequeueValues())
                {
                    try
                    {
                        if(!StatusCode.IsGood(value.StatusCode))
                        {
                            continue;
                        }

                        if(!propertyMap.TryGetValue(item.DisplayName, out var pinfo) || !pinfo.CanWrite)
                        {
                            Debug.WriteLine($"[OPC UA] Skip monitored notification. Property not found or not writable. Item={item.DisplayName}");
                            continue;
                        }

                        if(!TryConvertMonitoredValue(value.Value, pinfo.PropertyType, out var convertedValue))
                        {
                            Debug.WriteLine($"[OPC UA] Skip monitored notification. Value conversion failed. Item={item.DisplayName}, ValueType={value.Value?.GetType().FullName ?? "null"}, TargetType={pinfo.PropertyType.FullName}");
                            continue;
                        }

                        pinfo.SetValue(targetInstance, convertedValue);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"[OPC UA] Failed to process monitored notification. Item={item.DisplayName}, Error={ex}");
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("[OPC UA] Failed to dequeue monitored notification. " + ex);
            }
        }

        /// <summary>
        /// MonitoredValue 값 변환 함수.
        /// </summary>
        /// <param name="value">변환 대상 MonitoredValue</param>
        /// <param name="targetType">MonitoredValue 변환 타입</param>
        /// <param name="convertedValue">변환값</param>
        /// <returns>성공 시 True</returns>
        private bool TryConvertMonitoredValue(object value, Type targetType, out object convertedValue)
        {
            convertedValue = null;
            Type nullableType = Nullable.GetUnderlyingType(targetType);
            Type conversionType = nullableType ?? targetType;

            if(value == null)
            {
                return nullableType != null || !targetType.IsValueType;
            }

            if(targetType.IsInstanceOfType(value))
            {
                convertedValue = value;
                return true;
            }

            try
            {
                if(conversionType.IsEnum)
                {
                    convertedValue = value is string text
                        ? Enum.Parse(conversionType, text)
                        : Enum.ToObject(conversionType, value);
                    return true;
                }

                convertedValue = Convert.ChangeType(value, conversionType);
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"[OPC UA] Monitored value conversion exception. {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opc Ua Server Node의 데이터 타입 반환
        /// </summary>
        /// <param name="id">노드 ID</param>
        /// <returns>노드의 DataType</returns>
        private Type GetNodeDataType(NodeId id)
        {
            //var value = Session.ReadValue(id);
            var value = Session.ReadValueAsync(id).GetAwaiter().GetResult();
            var type = value.WrappedValue.TypeInfo.BuiltInType;
            var rank = value.WrappedValue.TypeInfo.ValueRank;
            return Opc.Ua.TypeInfo.GetSystemType(type, rank);
        }

        /// <summary>
        /// Opc Ua Server Node에 Write 할 값을, 해당 Node의 데이터 타입에 맞게 변환.
        /// </summary>
        /// <param name="type">타입</param>
        /// <param name="inputValue">입력값</param>
        /// <returns></returns>
        private object ConvertValueByNodeType(Type type, object inputValue)
        {
            if(inputValue is null)
            {
                return null;
            }

            if(type.IsInstanceOfType(inputValue))
            {
                return inputValue;
            }

            object outputValue = null;

            if(type == typeof(bool))
            {
                outputValue = Convert.ToBoolean((inputValue.ToString() == "True" ? 1 : 0));
            }
            else if(type == typeof(Int16))
            {
                outputValue = Convert.ToInt16(inputValue);
            }
            else if(type == typeof(Int32))
            {
                outputValue = Convert.ToInt32(inputValue);
            }
            else if(type == typeof(UInt16))
            {
                outputValue = Convert.ToUInt16(inputValue);
            }
            else if(type == typeof(UInt32))
            {
                outputValue = Convert.ToUInt32(inputValue);
            }
            else if(type == typeof(float))
            {
                outputValue = Convert.ToSingle(inputValue);
            }
            else if(type == typeof(double))
            {
                outputValue = Convert.ToDouble(inputValue);
            }
            else if(type == typeof(string))
            {
                outputValue = inputValue.ToString();
            }
            else if(type == typeof(DateTime))
            {
                if(inputValue.GetType() == typeof(double))
                {
                    TimeSpan time = TimeSpan.FromSeconds(Convert.ToDouble(inputValue));
                    DateTime date = DateTime.Today.Add(time);
                    outputValue = date.ToLongDateString();
                }
            }

            return outputValue;
        }
    }
}
