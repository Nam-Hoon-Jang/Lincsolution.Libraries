using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lincsolution.Core.Rendering.ThreeMF
{
    public enum eObjectType
    {
        Other = 0,
        Model = 1,
        Support = 2,
        SolidSupport = 3
    }

    public struct sTriangle
    {
        public UInt32[] Indices;
    }

    public struct sPosition
    {
        public Single[] Coordinates;
    }

    public struct sTransform
    {
        public Single[][] Fields;
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public unsafe struct InternalTransform
    {
        [FieldOffset(0)] public fixed Single Fields[12];
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public unsafe struct InternalPosition
    {
        [FieldOffset(0)] public fixed Single Coordinates[3];
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public unsafe struct InternalTriangle
    {
        [FieldOffset(0)] public fixed UInt32 Indices[3];
    }

    public sealed class Lib3mfWrapper
    {
        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_acquire", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static Int32 Acquire(IntPtr AInstance);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_base_classtypeid", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Base_ClassTypeId(IntPtr Handle, out UInt64 AClassTypeId);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_builditem_getobjectresource", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 BuildItem_GetObjectResource(IntPtr Handle, out IntPtr AObjectResource);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_builditem_getobjecttransform", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 BuildItem_GetObjectTransform(IntPtr Handle, out InternalTransform ATransform);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_builditem_getpartnumber", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 BuildItem_GetPartNumber(IntPtr Handle, UInt32 sizePartNumber, out UInt32 neededPartNumber, IntPtr dataPartNumber);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_builditem_hasobjecttransform", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 BuildItem_HasObjectTransform(IntPtr Handle, out Byte AHasTransform);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_builditemiterator_count", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 BuildItemIterator_Count(IntPtr Handle, out UInt64 ACount);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_builditemiterator_getcurrent", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 BuildItemIterator_GetCurrent(IntPtr Handle, out IntPtr ABuildItem);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_builditemiterator_movenext", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 BuildItemIterator_MoveNext(IntPtr Handle, out Byte AHasNext);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_createmodel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static Int32 CreateModel(out IntPtr AModel);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_getlasterror", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static Int32 GetLastError(IntPtr AInstance, UInt32 sizeLastErrorString, out UInt32 neededLastErrorString, IntPtr dataLastErrorString, out Byte AHasLastError);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_release", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static Int32 Release(IntPtr AInstance);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_reader_readfromfile", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Reader_ReadFromFile(IntPtr Handle, byte[] AFilename);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_reader_readfrombuffer", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Reader_ReadFromBuffer(IntPtr Handle, UInt64 sizeBuffer, IntPtr dataBuffer);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_reader_setprogresscallback", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Reader_SetProgressCallback(IntPtr Handle, IntPtr AProgressCallback, IntPtr AProgressUserData);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_resourceiterator_movenext", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 ResourceIterator_MoveNext(IntPtr Handle, out Byte AHasNext);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_resource_getresourceid", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Resource_GetResourceID(IntPtr Handle, out UInt32 AUniqueResourceID);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_resource_getmodelresourceid", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Resource_GetModelResourceID(IntPtr Handle, out UInt32 AModelResourceId);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_resourceiterator_getcurrent", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 ResourceIterator_GetCurrent(IntPtr Handle, out IntPtr AResource);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_resourceiterator_count", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 ResourceIterator_Count(IntPtr Handle, out UInt64 ACount);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_meshobject_getvertexcount", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 MeshObject_GetVertexCount(IntPtr Handle, out UInt32 AVertexCount);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_meshobject_gettrianglecount", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 MeshObject_GetTriangleCount(IntPtr Handle, out UInt32 AVertexCount);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_meshobject_getvertex", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 MeshObject_GetVertex(IntPtr Handle, UInt32 AIndex, out InternalPosition ACoordinates);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_meshobject_getvertices", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 MeshObject_GetVertices(IntPtr Handle, UInt64 sizeVertices, out UInt64 neededVertices, IntPtr dataVertices);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_meshobject_gettriangle", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 MeshObject_GetTriangle(IntPtr Handle, UInt32 AIndex, out InternalTriangle AIndices);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_meshobject_gettriangleindices", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 MeshObject_GetTriangleIndices(IntPtr Handle, UInt64 sizeIndices, out UInt64 neededIndices, IntPtr dataIndices);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_meshobjectiterator_getcurrentmeshobject", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 MeshObjectIterator_GetCurrentMeshObject(IntPtr Handle, out IntPtr AResource);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_model_getbuilditems", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Model_GetBuildItems(IntPtr Handle, out IntPtr ABuildItemIterator);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_model_getresources", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Model_GetResources(IntPtr Handle, out IntPtr AResourceIterator);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_model_getobjects", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Model_GetObjects(IntPtr Handle, out IntPtr AResourceIterator);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_model_getmeshobjects", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Model_GetMeshObjects(IntPtr Handle, out IntPtr AResourceIterator);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_model_queryreader", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Model_QueryReader(IntPtr Handle, byte[] AReaderClass, out IntPtr AReaderInstance);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_object_getname", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Object_GetName(IntPtr Handle, UInt32 sizeName, out UInt32 neededName, IntPtr dataName);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_object_getpartnumber", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Object_GetPartNumber(IntPtr Handle, UInt32 sizePartNumber, out UInt32 neededPartNumber, IntPtr dataPartNumber);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_object_gettype", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Object_GetType(IntPtr Handle, out Int32 AObjectType);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_object_iscomponentsobject", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Object_IsComponentsObject(IntPtr Handle, out Byte AIsComponentsObject);

        [DllImport("lib3mf.dll", EntryPoint = "lib3mf_object_ismeshobject", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Int32 Object_IsMeshObject(IntPtr Handle, out Byte AIsMeshObject);


        public static void ThrowError(IntPtr Handle, Int32 errorCode)
        {
            String sMessage = "Lib3MF Error";
            if(Handle != IntPtr.Zero)
            {
                UInt32 sizeMessage = 0;
                UInt32 neededMessage = 0;
                Byte hasLastError = 0;
                Int32 resultCode1 = GetLastError (Handle, sizeMessage, out neededMessage, IntPtr.Zero, out hasLastError);
                if((resultCode1 == 0) && (hasLastError != 0))
                {
                    sizeMessage = neededMessage;
                    byte[] bytesMessage = new byte[sizeMessage];

                    GCHandle dataMessage = GCHandle.Alloc(bytesMessage, GCHandleType.Pinned);
                    Int32 resultCode2 = GetLastError(Handle, sizeMessage, out neededMessage, dataMessage.AddrOfPinnedObject(), out hasLastError);
                    dataMessage.Free();

                    if((resultCode2 == 0) && (hasLastError != 0))
                    {
                        sMessage = sMessage + ": " + Encoding.UTF8.GetString(bytesMessage).TrimEnd(char.MinValue);
                    }
                }
            }

            throw new Exception(sMessage + "(# " + errorCode + ")");
        }

        public static T PolymorphicFactory<T>(IntPtr Handle) where T : class
        {
            T Object;
            if(Handle == IntPtr.Zero)
                return System.Activator.CreateInstance(typeof(T), Handle) as T;

            UInt64 resultClassTypeId = 0;
            Int32 errorCode = Base_ClassTypeId (Handle, out resultClassTypeId);
            if(errorCode != 0)
                ThrowError(IntPtr.Zero, errorCode);
            switch(resultClassTypeId)
            {
                case 0x856632D0BAF1D8B7:
                    Object = new CBase(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::Base"
                case 0x2D86831DA59FBE72:
                    Object = new CReader(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::Reader"
                case 0xDFE3889D1B269CBB:
                    Object = new CResource(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::Resource"
                case 0x460F3515E2621DBE:
                    Object = new CResourceIterator(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::ResourceIterator"
                case 0xF4196034E2B9FDE6:
                    Object = new CMeshObjectIterator(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::MeshObjectIterator"
                case 0x2DA2136F577A779C:
                    Object = new CObject(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::Object"
                case 0x3B3A6DC6EC610497:
                    Object = new CMeshObject(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::MeshObject"
                case 0x68FB2D5FFC4BA12A:
                    Object = new CBuildItem(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::BuildItem"
                case 0xA7D21BD364910860:
                    Object = new CBuildItemIterator(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::BuildItemIterator"
                case 0x5A8164ECEDB03F09:
                    Object = new CModel(Handle) as T;
                    break; // First 64 bits of SHA1 of a string: "Lib3MF::Model"
                default:
                    Object = System.Activator.CreateInstance(typeof(T), Handle) as T;
                    break;
            }
            return Object;
        }

        public static CModel CreateModel()
        {
            IntPtr newModel = IntPtr.Zero;

            CheckError(CreateModel(out newModel));
            return PolymorphicFactory<CModel>(newModel);
        }

        public static void Release(CBase AInstance)
        {
            IntPtr AInstanceHandle = IntPtr.Zero;
            if(AInstance != null)
                AInstanceHandle = AInstance.GetHandle();

            CheckError(Release(AInstanceHandle));
        }

        public static void Acquire(CBase AInstance)
        {
            IntPtr AInstanceHandle = IntPtr.Zero;
            if(AInstance != null)
                AInstanceHandle = AInstance.GetHandle();

            CheckError(Acquire(AInstanceHandle));
        }

        public unsafe static sTransform ConvertInternalToStruct_Transform(InternalTransform intTransform)
        {
            sTransform Transform;
            Transform.Fields = new Single[4][];
            for(int colIndex = 0; colIndex < 4; colIndex++)
            {
                Transform.Fields[colIndex] = new Single[3];
                for(int rowIndex = 0; rowIndex < 3; rowIndex++)
                {
                    Transform.Fields[colIndex][rowIndex] = intTransform.Fields[colIndex * 3 + rowIndex];
                }
            }

            return Transform;
        }

        public unsafe static sPosition convertInternalToStruct_Position(InternalPosition intPosition)
        {
            sPosition Position;
            Position.Coordinates = new Single[3];
            for(int rowIndex = 0; rowIndex < 3; rowIndex++)
            {
                Position.Coordinates[rowIndex] = intPosition.Coordinates[rowIndex];
            }

            return Position;
        }

        public unsafe static sTriangle convertInternalToStruct_Triangle(InternalTriangle intTriangle)
        {
            sTriangle Triangle;
            Triangle.Indices = new UInt32[3];
            for(int rowIndex = 0; rowIndex < 3; rowIndex++)
            {
                Triangle.Indices[rowIndex] = intTriangle.Indices[rowIndex];
            }

            return Triangle;
        }

        private static void CheckError(Int32 errorCode)
        {
            if(errorCode != 0)
            {
                ThrowError(IntPtr.Zero, errorCode);
            }
        }
    }

    public class CBase : IDisposable
    {
        protected IntPtr Handle;

        public CBase(IntPtr NewHandle)
        {
            Handle = NewHandle;
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                // dispose managed state (managed objects).
            }

            Lib3mfWrapper.Release(Handle);
            Handle = IntPtr.Zero;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        ~CBase() => Dispose(false);

        protected void CheckError(Int32 errorCode)
        {
            if(errorCode != 0)
            {
                Lib3mfWrapper.ThrowError(Handle, errorCode);
            }
        }

        public IntPtr GetHandle()
        {
            return Handle;
        }

        public UInt64 ClassTypeId()
        {
            UInt64 resultClassTypeId = 0;

            CheckError(Lib3mfWrapper.Base_ClassTypeId(Handle, out resultClassTypeId));
            return resultClassTypeId;
        }
    }

    public class CReader : CBase
    {
        public CReader(IntPtr NewHandle) : base(NewHandle) { }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeProgressCallback(out byte abort, double progress, int stage, IntPtr userData);

        public void ReadFromFile(String AFilename)
        {
            byte[] byteFilename = Encoding.UTF8.GetBytes(AFilename + char.MinValue);

            CheckError(Lib3mfWrapper.Reader_ReadFromFile(Handle, byteFilename));
        }

        public void ReadFromFile(String AFilename, Action<double, int> AProgressCallback)
        {
            if(AProgressCallback is null)
            {
                throw new ArgumentNullException("Action for Progress Callback is null", nameof(AProgressCallback));
            }

            Exception callbackError = null;

            NativeProgressCallback callback = (out byte abort, double progress, int stage, IntPtr userData) =>
            {
                abort = 0;

                try
                {
                    AProgressCallback(progress, stage);
                }
                catch(Exception ex)
                {
                    callbackError = ex;
                    abort = 1;
                }
            };

            IntPtr callbackPointer = Marshal.GetFunctionPointerForDelegate(callback);

            CheckError(Lib3mfWrapper.Reader_SetProgressCallback(Handle, callbackPointer, IntPtr.Zero));

            try
            {
                ReadFromFile(AFilename);
            }
            catch(Exception) when(callbackError != null)
            {
                throw new InvalidOperationException("Progress callback failed", callbackError);
            }
            finally
            {
                try
                {
                    CheckError(Lib3mfWrapper.Reader_SetProgressCallback(Handle, IntPtr.Zero, IntPtr.Zero));
                }
                finally
                {
                    GC.KeepAlive(callback);
                }
            }

            if(callbackError != null)
            {
                throw new InvalidOperationException("Progress callback failed", callbackError);
            }
        }

        public void ReadFromBuffer(Byte[] ABuffer)
        {
            GCHandle dataBuffer = GCHandle.Alloc(ABuffer, GCHandleType.Pinned);

            CheckError(Lib3mfWrapper.Reader_ReadFromBuffer(Handle, (UInt64)ABuffer.Length, dataBuffer.AddrOfPinnedObject()));
            dataBuffer.Free();
        }
    }

    public class CResource : CBase
    {
        public CResource(IntPtr NewHandle) : base(NewHandle) { }

        public UInt32 GetResourceID()
        {
            UInt32 resultUniqueResourceID = 0;

            CheckError(Lib3mfWrapper.Resource_GetResourceID(Handle, out resultUniqueResourceID));
            return resultUniqueResourceID;
        }

        public UInt32 GetModelResourceID()
        {
            UInt32 resultModelResourceId = 0;

            CheckError(Lib3mfWrapper.Resource_GetModelResourceID(Handle, out resultModelResourceId));
            return resultModelResourceId;
        }
    }

    public class CResourceIterator : CBase
    {
        public CResourceIterator(IntPtr NewHandle) : base(NewHandle) { }

        public bool MoveNext()
        {
            Byte resultHasNext = 0;

            CheckError(Lib3mfWrapper.ResourceIterator_MoveNext(Handle, out resultHasNext));
            return (resultHasNext != 0);
        }

        public CResource GetCurrent()
        {
            IntPtr newResource = IntPtr.Zero;

            CheckError(Lib3mfWrapper.ResourceIterator_GetCurrent(Handle, out newResource));
            return Lib3mfWrapper.PolymorphicFactory<CResource>(newResource);
        }

        public UInt64 Count()
        {
            UInt64 resultCount = 0;

            CheckError(Lib3mfWrapper.ResourceIterator_Count(Handle, out resultCount));
            return resultCount;
        }
    }

    public class CModel : CBase
    {
        public CModel(IntPtr NewHandle) : base(NewHandle) { }

        public CReader QueryReader(String AReaderClass)
        {
            byte[] byteReaderClass = Encoding.UTF8.GetBytes(AReaderClass + char.MinValue);
            IntPtr newReaderInstance = IntPtr.Zero;

            CheckError(Lib3mfWrapper.Model_QueryReader(Handle, byteReaderClass, out newReaderInstance));
            return Lib3mfWrapper.PolymorphicFactory<CReader>(newReaderInstance);
        }

        public CBuildItemIterator GetBuildItems()
        {
            IntPtr newBuildItemIterator = IntPtr.Zero;

            CheckError(Lib3mfWrapper.Model_GetBuildItems(Handle, out newBuildItemIterator));
            return Lib3mfWrapper.PolymorphicFactory<CBuildItemIterator>(newBuildItemIterator);
        }


        public CMeshObjectIterator GetMeshObjects()
        {
            IntPtr newResourceIterator = IntPtr.Zero;

            CheckError(Lib3mfWrapper.Model_GetMeshObjects(Handle, out newResourceIterator));
            return Lib3mfWrapper.PolymorphicFactory<CMeshObjectIterator>(newResourceIterator);
        }
    }

    public class CObject : CResource
    {
        public CObject(IntPtr NewHandle) : base(NewHandle) { }

        public eObjectType GetType()
        {
            Int32 resultObjectType = 0;

            CheckError(Lib3mfWrapper.Object_GetType(Handle, out resultObjectType));
            return (eObjectType)(resultObjectType);
        }

        public String GetName()
        {
            UInt32 sizeName = 0;
            UInt32 neededName = 0;
            CheckError(Lib3mfWrapper.Object_GetName(Handle, sizeName, out neededName, IntPtr.Zero));
            sizeName = neededName;
            byte[] bytesName = new byte[sizeName];
            GCHandle dataName = GCHandle.Alloc(bytesName, GCHandleType.Pinned);

            CheckError(Lib3mfWrapper.Object_GetName(Handle, sizeName, out neededName, dataName.AddrOfPinnedObject()));
            dataName.Free();
            return Encoding.UTF8.GetString(bytesName).TrimEnd(char.MinValue);
        }

        public String GetPartNumber()
        {
            UInt32 sizePartNumber = 0;
            UInt32 neededPartNumber = 0;
            CheckError(Lib3mfWrapper.Object_GetPartNumber(Handle, sizePartNumber, out neededPartNumber, IntPtr.Zero));
            sizePartNumber = neededPartNumber;
            byte[] bytesPartNumber = new byte[sizePartNumber];
            GCHandle dataPartNumber = GCHandle.Alloc(bytesPartNumber, GCHandleType.Pinned);

            CheckError(Lib3mfWrapper.Object_GetPartNumber(Handle, sizePartNumber, out neededPartNumber, dataPartNumber.AddrOfPinnedObject()));
            dataPartNumber.Free();
            return Encoding.UTF8.GetString(bytesPartNumber).TrimEnd(char.MinValue);
        }

        public bool IsMeshObject()
        {
            Byte resultIsMeshObject = 0;

            CheckError(Lib3mfWrapper.Object_IsMeshObject(Handle, out resultIsMeshObject));
            return (resultIsMeshObject != 0);
        }

        public bool IsComponentsObject()
        {
            Byte resultIsComponentsObject = 0;

            CheckError(Lib3mfWrapper.Object_IsComponentsObject(Handle, out resultIsComponentsObject));
            return (resultIsComponentsObject != 0);
        }
    }

    public class CBuildItem : CBase
    {
        public CBuildItem(IntPtr NewHandle) : base(NewHandle) { }


        public CObject GetObjectResource()
        {
            IntPtr newObjectResource = IntPtr.Zero;

            CheckError(Lib3mfWrapper.BuildItem_GetObjectResource(Handle, out newObjectResource));
            return Lib3mfWrapper.PolymorphicFactory<CObject>(newObjectResource);
        }

        public bool HasObjectTransform()
        {
            Byte resultHasTransform = 0;

            CheckError(Lib3mfWrapper.BuildItem_HasObjectTransform(Handle, out resultHasTransform));
            return (resultHasTransform != 0);
        }

        public sTransform GetObjectTransform()
        {
            InternalTransform intresultTransform;

            CheckError(Lib3mfWrapper.BuildItem_GetObjectTransform(Handle, out intresultTransform));
            return Lib3mfWrapper.ConvertInternalToStruct_Transform(intresultTransform);
        }

        public String GetPartNumber()
        {
            UInt32 sizePartNumber = 0;
            UInt32 neededPartNumber = 0;
            CheckError(Lib3mfWrapper.BuildItem_GetPartNumber(Handle, sizePartNumber, out neededPartNumber, IntPtr.Zero));
            sizePartNumber = neededPartNumber;
            byte[] bytesPartNumber = new byte[sizePartNumber];
            GCHandle dataPartNumber = GCHandle.Alloc(bytesPartNumber, GCHandleType.Pinned);

            CheckError(Lib3mfWrapper.BuildItem_GetPartNumber(Handle, sizePartNumber, out neededPartNumber, dataPartNumber.AddrOfPinnedObject()));
            dataPartNumber.Free();
            return Encoding.UTF8.GetString(bytesPartNumber).TrimEnd(char.MinValue);
        }
    }

    public class CBuildItemIterator : CBase
    {
        public CBuildItemIterator(IntPtr NewHandle) : base(NewHandle)
        {

        }

        public bool MoveNext()
        {
            Byte resultHasNext = 0;

            CheckError(Lib3mfWrapper.BuildItemIterator_MoveNext(Handle, out resultHasNext));
            return (resultHasNext != 0);
        }

        public CBuildItem GetCurrent()
        {
            IntPtr newBuildItem = IntPtr.Zero;

            CheckError(Lib3mfWrapper.BuildItemIterator_GetCurrent(Handle, out newBuildItem));
            return Lib3mfWrapper.PolymorphicFactory<CBuildItem>(newBuildItem);
        }

        public UInt64 Count()
        {
            UInt64 resultCount = 0;

            CheckError(Lib3mfWrapper.BuildItemIterator_Count(Handle, out resultCount));
            return resultCount;
        }
    }

    public class CMeshObject : CObject
    {
        public CMeshObject(IntPtr NewHandle) : base(NewHandle) { }

        public UInt32 GetVertexCount()
        {
            UInt32 resultVertexCount = 0;

            CheckError(Lib3mfWrapper.MeshObject_GetVertexCount(Handle, out resultVertexCount));
            return resultVertexCount;
        }

        public UInt32 GetTriangleCount()
        {
            UInt32 resultVertexCount = 0;

            CheckError(Lib3mfWrapper.MeshObject_GetTriangleCount(Handle, out resultVertexCount));
            return resultVertexCount;
        }

        public sPosition GetVertex(UInt32 AIndex)
        {
            InternalPosition intresultCoordinates;

            CheckError(Lib3mfWrapper.MeshObject_GetVertex(Handle, AIndex, out intresultCoordinates));
            return Lib3mfWrapper.convertInternalToStruct_Position(intresultCoordinates);
        }

        public void GetVertices(out sPosition[] AVertices)
        {
            UInt64 sizeVertices = 0;
            UInt64 neededVertices = 0;
            CheckError(Lib3mfWrapper.MeshObject_GetVertices(Handle, sizeVertices, out neededVertices, IntPtr.Zero));
            sizeVertices = neededVertices;
            var arrayVertices = new InternalPosition[sizeVertices];
            GCHandle dataVertices = GCHandle.Alloc(arrayVertices, GCHandleType.Pinned);

            CheckError(Lib3mfWrapper.MeshObject_GetVertices(Handle, sizeVertices, out neededVertices, dataVertices.AddrOfPinnedObject()));
            dataVertices.Free();
            AVertices = new sPosition[sizeVertices];
            for(int index = 0; index < AVertices.Length; index++)
                AVertices[index] = Lib3mfWrapper.convertInternalToStruct_Position(arrayVertices[index]);
        }

        public sTriangle GetTriangle(UInt32 AIndex)
        {
            InternalTriangle intresultIndices;

            CheckError(Lib3mfWrapper.MeshObject_GetTriangle(Handle, AIndex, out intresultIndices));
            return Lib3mfWrapper.convertInternalToStruct_Triangle(intresultIndices);
        }

        public void GetTriangleIndices(out sTriangle[] AIndices)
        {
            UInt64 sizeIndices = 0;
            UInt64 neededIndices = 0;
            CheckError(Lib3mfWrapper.MeshObject_GetTriangleIndices(Handle, sizeIndices, out neededIndices, IntPtr.Zero));
            sizeIndices = neededIndices;
            var arrayIndices = new InternalTriangle[sizeIndices];
            GCHandle dataIndices = GCHandle.Alloc(arrayIndices, GCHandleType.Pinned);

            CheckError(Lib3mfWrapper.MeshObject_GetTriangleIndices(Handle, sizeIndices, out neededIndices, dataIndices.AddrOfPinnedObject()));
            dataIndices.Free();
            AIndices = new sTriangle[sizeIndices];
            for(int index = 0; index < AIndices.Length; index++)
                AIndices[index] = Lib3mfWrapper.convertInternalToStruct_Triangle(arrayIndices[index]);
        }
    }

    public class CMeshObjectIterator : CResourceIterator
    {
        public CMeshObjectIterator(IntPtr NewHandle) : base(NewHandle) { }

        public CMeshObject GetCurrentMeshObject()
        {
            IntPtr newResource = IntPtr.Zero;

            CheckError(Lib3mfWrapper.MeshObjectIterator_GetCurrentMeshObject(Handle, out newResource));
            return Lib3mfWrapper.PolymorphicFactory<CMeshObject>(newResource);
        }
    }
}
