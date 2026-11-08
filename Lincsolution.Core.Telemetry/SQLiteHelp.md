# SQLiteHelp

SQLite에서 자주 사용하는 SQL 명령과 구문을 용도별로 정리한 참고 문서.

## 목차

1. [기본 개념](#기본-개념)
2. [데이터 읽기·추가·수정·삭제](#데이터-읽기추가수정삭제)
3. [조회 구문](#조회-구문)
4. [조건식과 연산자](#조건식과-연산자)
5. [계산·집계 함수](#계산집계-함수)
6. [여러 테이블·조회 결과 연결](#여러-테이블조회-결과-연결)
7. [테이블과 검색 구조 관리](#테이블과-검색-구조-관리)
8. [중복 처리와 UPSERT](#중복-처리와-upsert)
9. [트랜잭션](#트랜잭션)
10. [SQLite 설정·점검](#sqlite-설정점검)
11. [C# 실행 메서드](#c-실행-메서드)
12. [추가 기능과 학습 순서](#추가-기능과-학습-순서)

## 기본 개념

- **SQL**: 데이터를 정의·저장·조회·수정하는 언어.
- **SQLite**: SQL을 해석하고 실행하는 데이터베이스 엔진.
- **데이터베이스**: 테이블과 인덱스 등을 담는 저장소. SQLite는 파일 기반으로 사용 가능.
- **테이블**: 행과 열로 구성된 데이터 집합.
- **행(Row)**: 개별 데이터 한 건.
- **열(Column)**: 이름·나이 등 데이터 항목.

```text
C#에서 SQL 명령과 매개변수 전달
→ 앱 내부의 SQLite 엔진이 실행
→ DB 파일 읽기·쓰기
→ 조회 결과 또는 처리 결과 반환
```

별도 DB 서버에 네트워크로 명령을 보내는 구조가 아니다. SQL 문자열을 지정한 뒤 실행 메서드를 호출해야 실제 동작한다.

아래 예제는 문법별 참고용이다. 전체를 위에서부터 한 번에 실행하는 스크립트가 아니다. Students, Classes, Accounts 등은 각 예제에 필요한 테이블이 존재한다고 가정한다.

## 데이터 읽기·추가·수정·삭제

| 명령 | 역할 | 예시 |
|---|---|---|
| `SELECT` | 데이터 조회 | `SELECT Name, Age FROM Students;` |
| `INSERT INTO` | 행 추가 | `INSERT INTO Students (Name, Age) VALUES ('민수', 20);` |
| `UPDATE` | 기존 값 수정 | `UPDATE Students SET Age = 21 WHERE Id = 1;` |
| `DELETE FROM` | 행 삭제 | `DELETE FROM Students WHERE Id = 1;` |

**UPDATE와 DELETE에서 WHERE를 생략하면 전체 행에 적용된다.** SELECT는 저장된 데이터를 변경하지 않는다.

## 조회 구문

WHERE 등은 단독 명령이 아니라 SELECT 등의 명령에 붙이는 구문이다.

| 구문 | 의미 | 사용 예 |
|---|---|---|
| `FROM` | 조회 대상 테이블 | `FROM Students` |
| `WHERE` | 조건에 맞는 행 선택 | `WHERE Age >= 20` |
| `ORDER BY` | 결과 정렬 | `ORDER BY Age DESC, Id ASC` |
| `ASC` / `DESC` | 오름차순 / 내림차순 | `ORDER BY Age DESC` |
| `LIMIT` | 결과 개수 제한 | `LIMIT 10` |
| `OFFSET` | 앞의 결과 일부 건너뛰기 | `LIMIT 10 OFFSET 20` |
| `DISTINCT` | 결과 중복 제거 | `SELECT DISTINCT Age FROM Students;` |
| `AS` | 열·테이블에 별칭 부여 | `SELECT Name AS StudentName FROM Students;` |
| `GROUP BY` | 같은 값끼리 묶어 집계 | `GROUP BY Age` |
| `HAVING` | 집계 결과에 조건 적용 | `HAVING COUNT(*) >= 2` |

```sql
SELECT Id, Name, Age
FROM Students
WHERE Age >= 20
ORDER BY Age DESC, Id ASC
LIMIT 10;
```

20세 이상 학생 중 나이가 많은 순으로 최대 10명 조회. 나이가 같으면 Id 오름차순으로 정렬한다. **ORDER BY가 없으면 결과 순서는 보장되지 않는다.**

[공식 SELECT 설명](https://www.sqlite.org/lang_select.html)

## 조건식과 연산자

| 표현 | 의미 | 예시 |
|---|---|---|
| `=`, `!=`, `<>` | 같음 / 다름 | `Age <> 20` |
| `>`, `>=`, `<`, `<=` | 크기 비교 | `Age >= 20` |
| `AND` | 두 조건 모두 충족 | `Age >= 20 AND Age < 30` |
| `OR` | 하나 이상의 조건 충족 | `Age = 20 OR Age = 30` |
| `NOT` | 조건 부정 | `NOT (Age = 20)` |
| `BETWEEN` | 양 끝을 포함하는 범위 | `Age BETWEEN 20 AND 29` |
| `IN` | 목록 중 하나와 일치 | `Age IN (20, 25, 30)` |
| `LIKE` | 문자열 패턴 일치 | `Name LIKE '김%'` |
| `IS NULL` | 값이 없음 | `Age IS NULL` |
| `IS NOT NULL` | 값이 있음 | `Age IS NOT NULL` |
| `EXISTS` | 하위 조회 결과 존재 | `EXISTS (SELECT 1 FROM Students)` |

LIKE에서 `%`는 0개 이상의 문자, `_`는 문자 하나를 뜻한다.

```sql
SELECT * FROM Students WHERE Name LIKE '김%';   -- 김으로 시작
SELECT * FROM Students WHERE Name LIKE '%수';   -- 수로 끝남
SELECT * FROM Students WHERE Name LIKE '%민%';  -- 민이 포함됨
```

**NULL 검사는 `= NULL`이 아니라 `IS NULL`을 사용한다.** NULL은 빈 문자열이나 숫자 0과 다른 상태다.

[공식 표현식 설명](https://www.sqlite.org/lang_expr.html)

## 계산·집계 함수

| 함수 | 역할 |
|---|---|
| `COUNT(*)` | 행 개수 |
| `COUNT(Age)` | Age가 NULL이 아닌 행 개수 |
| `SUM(Age)` | 합계 |
| `AVG(Age)` | 평균 |
| `MIN(Age)` / `MAX(Age)` | 최솟값 / 최댓값 |
| `ROUND(Value, 3)` | 소수점 셋째 자리로 반올림 |
| `ABS(Value)` | 절댓값 |
| `COALESCE(Age, 0)` | Age가 NULL이면 0 사용 |
| `LENGTH(Name)` | 문자열 길이 |
| `REPLACE(Name, '김', '이')` | 문자열 일부 치환 |
| `CAST(Value AS INTEGER)` | 자료형 변환 |

```sql
SELECT Age, COUNT(*) AS StudentCount
FROM Students
GROUP BY Age
HAVING COUNT(*) >= 2;
```

같은 나이끼리 묶고 학생이 2명 이상인 나이와 인원수 반환. WHERE는 집계 전 행을 거르고, HAVING은 집계 결과를 거른다. 위 집계 함수들은 NULL을 제외하며 COUNT(*)는 행 자체를 센다.

CASE는 조건에 따라 다른 값을 반환한다.

```sql
SELECT Name,
       CASE
           WHEN Age >= 20 THEN '성인'
           WHEN Age < 20 THEN '미성년'
           ELSE '나이 미입력'
       END AS AgeGroup
FROM Students;
```

[집계 함수](https://www.sqlite.org/lang_aggfunc.html) · [일반 함수](https://www.sqlite.org/lang_corefunc.html)

## 여러 테이블·조회 결과 연결

| 구문 | 역할 |
|---|---|
| `INNER JOIN` | 두 테이블에서 조건이 맞는 행끼리 연결 |
| `LEFT JOIN` | 왼쪽 행을 유지하고 오른쪽 일치 항목이 없으면 NULL |
| `CROSS JOIN` | 두 테이블의 모든 행 조합 |
| `UNION` | 두 조회 결과를 합치고 중복 제거 |
| `UNION ALL` | 두 조회 결과를 중복 유지하며 합침 |
| `INTERSECT` | 두 조회 결과에 공통으로 있는 행 |
| `EXCEPT` | 첫 조회 결과에만 있는 행 |
| `WITH` | 쿼리 안에서 사용할 임시 이름의 조회 결과 정의 |

```sql
SELECT s.Name, c.ClassName
FROM Students AS s
LEFT JOIN Classes AS c ON s.ClassId = c.Id;
```

학생의 ClassId와 반 테이블의 Id를 연결해 학생 이름과 반 이름을 조회한다. JOIN은 관련 테이블의 열을 연결하고, UNION은 호환되는 조회 결과의 행을 합친다. UNION 양쪽 SELECT의 열 개수는 같아야 한다.

[공식 조회 구문](https://www.sqlite.org/lang_select.html) · [WITH](https://www.sqlite.org/lang_with.html)

## 테이블과 검색 구조 관리

| 명령 | 역할 | 예시 |
|---|---|---|
| `CREATE TABLE` | 테이블 생성 | 아래 생성 예제 참고 |
| `ALTER TABLE ... ADD COLUMN` | 열 추가 | `ALTER TABLE Students ADD COLUMN Email TEXT;` |
| `ALTER TABLE ... RENAME TO` | 테이블 이름 변경 | `ALTER TABLE Students RENAME TO Members;` |
| `DROP TABLE` | 테이블과 데이터 삭제 | `DROP TABLE Students;` |
| `CREATE INDEX` | 검색용 인덱스 생성 | `CREATE INDEX IX_Students_Age ON Students(Age);` |
| `CREATE UNIQUE INDEX` | 중복을 제한하는 인덱스 생성 | `CREATE UNIQUE INDEX UX_Students_Email ON Students(Email);` |
| `DROP INDEX` | 인덱스 삭제 | `DROP INDEX IX_Students_Age;` |
| `CREATE VIEW` | 조회문에 이름을 붙여 저장 | `CREATE VIEW Adults AS SELECT * FROM Students WHERE Age >= 20;` |
| `DROP VIEW` | View 정의 삭제 | `DROP VIEW Adults;` |

View는 일반적으로 결과 데이터를 별도로 저장하는 테이블이 아니라 저장해 둔 조회문이다. 인덱스는 검색을 도울 수 있지만 저장 공간과 쓰기 비용도 늘린다.

### 제약조건

| 제약조건 | 의미 |
|---|---|
| `PRIMARY KEY` | 행을 고유하게 식별 |
| `NOT NULL` | NULL 금지 |
| `UNIQUE` | 중복 값 제한. SQLite에서는 여러 NULL 허용 |
| `DEFAULT` | 값 생략 시 기본값 |
| `CHECK` | 입력값 조건 검사 |
| `FOREIGN KEY` | 다른 테이블과의 참조 관계 제약 |

```sql
CREATE TABLE IF NOT EXISTS Students (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Age INTEGER CHECK (Age >= 0),
    Status TEXT NOT NULL DEFAULT 'Active'
);
```

- INTEGER PRIMARY KEY는 값을 생략하면 정수 ID를 자동 부여한다.
- IF NOT EXISTS는 같은 이름의 테이블이 있으면 생성을 건너뛴다. 기존 구조를 갱신하거나 데이터를 삭제하지 않는다.
- Age의 CHECK는 음수를 막지만 NULL을 금지하지는 않는다. 필수 값이면 NOT NULL도 지정한다.
- 외래 키 제약은 각 연결에서 `PRAGMA foreign_keys = ON;`으로 활성화해 사용하는 것이 명확하다.

[테이블 정의](https://www.sqlite.org/lang_createtable.html) · [외래 키 설정](https://www.sqlite.org/pragma.html#pragma_foreign_keys)

## 중복 처리와 UPSERT

고유 키가 없으면 추가하고, 이미 있으면 수정한다.

```sql
INSERT INTO Students (Id, Name, Age)
VALUES (1, '민수', 21)
ON CONFLICT(Id) DO UPDATE SET
    Name = excluded.Name,
    Age = excluded.Age;
```

`excluded`는 이번에 넣으려던 값을 가리킨다.

중복이면 아무것도 하지 않는 예제:

```sql
INSERT INTO Students (Id, Name, Age)
VALUES (1, '민수', 21)
ON CONFLICT(Id) DO NOTHING;
```

**INSERT OR REPLACE는 단순 UPDATE와 다르다.** 고유 제약 충돌 시 기존 행을 삭제한 뒤 삽입할 수 있다.

[UPSERT](https://www.sqlite.org/lang_upsert.html) · [충돌 처리](https://www.sqlite.org/lang_conflict.html)

## 트랜잭션

여러 변경을 하나의 작업 단위로 묶는다.

| 명령 | 역할 |
|---|---|
| `BEGIN` | 트랜잭션 시작 |
| `COMMIT` | 변경 확정 |
| `ROLLBACK` | 아직 확정하지 않은 변경 취소 |
| `SAVEPOINT 이름` | 내부 되돌리기 지점 생성 |
| `ROLLBACK TO 이름` | 해당 지점 이후 변경 취소 |
| `RELEASE 이름` | 저장 지점 해제 |

```sql
BEGIN;

UPDATE Accounts SET Balance = Balance - 100 WHERE Id = 1;
UPDATE Accounts SET Balance = Balance + 100 WHERE Id = 2;

COMMIT;
```

두 변경을 하나의 작업으로 확정한다. 취소하려면 COMMIT 대신 ROLLBACK을 실행한다. 이미 확정한 트랜잭션은 ROLLBACK으로 되돌릴 수 없다. 실제 송금 로직은 계좌 존재, 잔액 등 추가 검증이 필요하다.

[트랜잭션](https://www.sqlite.org/lang_transaction.html) · [SAVEPOINT](https://www.sqlite.org/lang_savepoint.html)

## SQLite 설정·점검

| 명령 | 역할 |
|---|---|
| `PRAGMA table_info(Students);` | 테이블 열 정보 확인 |
| `PRAGMA index_list(Students);` | 테이블 인덱스 목록 확인 |
| `PRAGMA foreign_keys = ON;` | 외래 키 제약 활성화 |
| `PRAGMA journal_mode = WAL;` | 기록 저널 방식을 WAL로 설정 |
| `PRAGMA busy_timeout = 5000;` | 잠금 충돌 시 대기 시간 설정. 단위 ms |
| `PRAGMA integrity_check;` | DB 구조 무결성 검사 |
| `EXPLAIN QUERY PLAN SELECT * FROM Students WHERE Age = 20;` | 조회 실행 계획 확인 |
| `ANALYZE;` | 쿼리 최적화에 사용하는 통계 수집 |
| `PRAGMA optimize;` | 필요에 따른 통계 관리 등 최적화 작업 |
| `VACUUM;` | DB 재구성 및 사용하지 않는 공간 회수 |
| `ATTACH DATABASE 'other.db' AS other;` | 다른 DB 파일을 현재 연결에 추가 |
| `DETACH DATABASE other;` | 추가한 DB 연결 해제 |

PRAGMA는 SQLite의 설정과 내부 정보를 다루는 구문이다. 모두 켜야 하는 필수 설정 목록은 아니다. WAL 사용 중에는 DB 옆에 `-wal`, `-shm` 보조 파일이 생길 수 있다.

SQLite에서는 일반적으로 서버형 DB의 CREATE DATABASE 명령 대신 파일 경로로 연결을 열어 DB를 생성한다. `.tables`, `.schema` 같은 명령은 SQLite CLI 전용 명령이며 C#의 SQL 명령으로 그대로 실행하는 구문이 아니다.

[PRAGMA](https://www.sqlite.org/pragma.html) · [VACUUM](https://www.sqlite.org/lang_vacuum.html) · [SQLite CLI](https://www.sqlite.org/cli.html)

## C# 실행 메서드

Microsoft.Data.Sqlite 기준으로 SQL과 실제 값을 별도로 전달한다.

```csharp
command.CommandText =
    "INSERT INTO Students (Name, Age) VALUES ($name, $age);";

command.Parameters.AddWithValue("$name", "민수");
command.Parameters.AddWithValue("$age", 20);

command.ExecuteNonQuery();
```

| 메서드 | 용도 |
|---|---|
| `ExecuteNonQuery()` | 테이블 생성, INSERT, UPDATE, DELETE 등 실행 |
| `ExecuteReader()` | 여러 행·열의 조회 결과 읽기 |
| `ExecuteScalar()` | 결과의 첫 행·첫 열 하나 읽기. COUNT(*) 조회 등에 사용 |

매개변수는 문자열 이어 붙이기 대신 값을 전달하는 방법이다. 따옴표·숫자 표기 문제와 SQL 삽입을 방지한다. 테이블명이나 열명은 값 매개변수로 대체할 수 없다.

```csharp
command.CommandText = "SELECT Name, Age FROM Students;";

using (var reader = command.ExecuteReader())
{
    while (reader.Read())
    {
        string name = reader.GetString(0);
        int? age = reader.IsDBNull(1)
            ? (int?)null
            : reader.GetInt32(1);
    }
}
```

0과 1은 SELECT에 나열한 열의 순서이며 행의 Id가 아니다. 여기서 `int?`는 nullable 값 형식으로 C# 7.3에서도 지원된다.

[Microsoft.Data.Sqlite 개요](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/)

## 추가 기능과 학습 순서

| 기능 | 용도 |
|---|---|
| `CREATE TRIGGER` / `DROP TRIGGER` | INSERT·UPDATE·DELETE 등에 따른 자동 동작 정의·삭제 |
| `RETURNING` | INSERT·UPDATE·DELETE로 처리한 행의 값 반환 |
| `OVER`, `ROW_NUMBER()`, `LAG()` | 행 간 순위·이전 값 등 계산 |
| 날짜·시간 함수 | 날짜 생성, 변환, 계산 |
| JSON 함수 | JSON 값 추출·구성 등 |

일부 기능은 사용하는 SQLite 엔진 버전과 빌드 구성에 따라 지원 여부가 다르다.

권장 학습 순서:

1. SELECT / INSERT / UPDATE / DELETE
2. WHERE / ORDER BY / LIMIT
3. 집계 함수 / GROUP BY
4. 트랜잭션
5. 인덱스 / JOIN
6. 필요에 따라 UPSERT, WITH, 윈도 함수 등

[SQLite 전체 SQL 목록](https://www.sqlite.org/lang.html)
