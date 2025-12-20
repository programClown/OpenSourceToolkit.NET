using CommunityToolkit.Mvvm.Input;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class SqlTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Query { get; set; }
    }

    public class SqlFormatterToolViewModel : ToolViewModel
    {
        public override int Id => 37;
        public override string Name => ToolkitLocalization.GetString("Tool_SqlFormatter_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_SqlFormatter_Description");
        public override string IconKey => "SqlFormatterIcon";

        private static readonly string[] BaseSqlKeywords = {
            "SELECT", "FROM", "WHERE", "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN",
            "CROSS JOIN", "NATURAL JOIN", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP",
            "TABLE", "INDEX", "VIEW", "AND", "OR", "NOT", "IN", "EXISTS", "BETWEEN", "LIKE",
            "IS NULL", "IS NOT NULL", "ORDER BY", "GROUP BY", "HAVING", "UNION", "UNION ALL",
            "INTERSECT", "EXCEPT", "CASE", "WHEN", "THEN", "ELSE", "END", "AS", "ON", "INTO",
            "VALUES", "SET", "COUNT", "SUM", "AVG", "MIN", "MAX", "DISTINCT", "ALL",
            "PRIMARY KEY", "FOREIGN KEY", "REFERENCES", "DEFAULT", "NOT NULL", "CONSTRAINT",
            "UNIQUE", "CHECK", "NULL", "TRUE", "FALSE", "ASC", "DESC", "NULLS FIRST", "NULLS LAST",
            "COMMIT", "ROLLBACK", "BEGIN", "TRANSACTION", "SAVEPOINT", "GRANT", "REVOKE",
            "TRUNCATE", "MERGE", "USING", "MATCHED", "WITH", "RECURSIVE", "OVER", "PARTITION BY",
            "ROW_NUMBER", "RANK", "DENSE_RANK", "NTILE", "LAG", "LEAD", "FIRST_VALUE", "LAST_VALUE",
            "COALESCE", "NULLIF", "CAST", "CONVERT", "TRIM", "UPPER", "LOWER", "SUBSTRING",
            "CONCAT", "LENGTH", "REPLACE", "ABS", "ROUND", "FLOOR", "CEILING", "CURRENT_TIMESTAMP",
            "CURRENT_DATE", "CURRENT_TIME"
        };

        private static readonly string[] BaseMajorKeywords = {
            "SELECT", "FROM", "WHERE", "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN",
            "CROSS JOIN", "ORDER BY", "GROUP BY", "HAVING", "UNION", "INTERSECT", "EXCEPT",
            "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP", "SET", "VALUES", "INTO",
            "WITH", "MERGE", "USING", "WHEN MATCHED", "WHEN NOT MATCHED"
        };

        private static readonly Dictionary<string, string[]> DialectKeywords = new Dictionary<string, string[]>
        {
            ["SQL Server"] = new[] {
                "TOP", "IDENTITY", "GETDATE", "GETUTCDATE", "SYSDATETIME", "SYSUTCDATETIME",
                "DATEADD", "DATEDIFF", "DATENAME", "DATEPART", "EOMONTH", "SWITCHOFFSET",
                "TODATETIMEOFFSET", "ISDATE", "ISNULL", "ISNUMERIC", "NEWID", "NEWSEQUENTIALID",
                "SCOPE_IDENTITY", "@@IDENTITY", "@@ROWCOUNT", "@@ERROR", "@@TRANCOUNT",
                "NVARCHAR", "NCHAR", "NTEXT", "VARCHAR", "CHAR", "TEXT", "VARBINARY", "BINARY",
                "IMAGE", "DATETIME", "DATETIME2", "DATETIMEOFFSET", "SMALLDATETIME", "DATE", "TIME",
                "BIGINT", "INT", "SMALLINT", "TINYINT", "BIT", "DECIMAL", "NUMERIC", "MONEY",
                "SMALLMONEY", "FLOAT", "REAL", "UNIQUEIDENTIFIER", "XML", "SQL_VARIANT",
                "GEOGRAPHY", "GEOMETRY", "HIERARCHYID", "ROWVERSION", "TIMESTAMP",
                "CLUSTERED", "NONCLUSTERED", "INCLUDE", "FILLFACTOR", "PAD_INDEX",
                "NOLOCK", "HOLDLOCK", "UPDLOCK", "TABLOCK", "TABLOCKX", "ROWLOCK", "PAGLOCK",
                "READUNCOMMITTED", "READCOMMITTED", "REPEATABLEREAD", "SERIALIZABLE", "SNAPSHOT",
                "OUTPUT", "INSERTED", "DELETED", "MERGE", "MATCHED",
                "TRY", "CATCH", "THROW", "RAISERROR", "PRINT",
                "EXEC", "EXECUTE", "SP_EXECUTESQL", "OPENQUERY", "OPENROWSET", "OPENDATASOURCE",
                "PIVOT", "UNPIVOT", "CROSS APPLY", "OUTER APPLY",
                "STRING_AGG", "STRING_SPLIT", "JSON_VALUE", "JSON_QUERY", "JSON_MODIFY",
                "FOR JSON", "FOR XML", "OPENJSON", "OPENXML",
                "FETCH NEXT", "OFFSET", "ROWS ONLY",
                "IIF", "CHOOSE", "TRY_CAST", "TRY_CONVERT", "TRY_PARSE", "PARSE", "FORMAT"
            },
            ["MySQL"] = new[] {
                "LIMIT", "OFFSET", "AUTO_INCREMENT", "ENGINE", "INNODB", "MYISAM",
                "NOW", "CURDATE", "CURTIME", "DATE_ADD", "DATE_SUB", "DATEDIFF", "DATE_FORMAT",
                "STR_TO_DATE", "TIMESTAMPDIFF", "TIMESTAMPADD", "UNIX_TIMESTAMP", "FROM_UNIXTIME",
                "IFNULL", "NULLIF", "IF", "CASE", "COALESCE",
                "VARCHAR", "CHAR", "TEXT", "TINYTEXT", "MEDIUMTEXT", "LONGTEXT",
                "BLOB", "TINYBLOB", "MEDIUMBLOB", "LONGBLOB", "BINARY", "VARBINARY",
                "INT", "TINYINT", "SMALLINT", "MEDIUMINT", "BIGINT", "FLOAT", "DOUBLE", "DECIMAL",
                "DATETIME", "DATE", "TIME", "TIMESTAMP", "YEAR",
                "ENUM", "SET", "JSON", "BOOLEAN", "BOOL",
                "UNSIGNED", "ZEROFILL", "ON UPDATE CURRENT_TIMESTAMP",
                "SHOW", "DESCRIBE", "EXPLAIN", "USE", "DATABASE", "DATABASES", "TABLES", "COLUMNS",
                "STRAIGHT_JOIN", "SQL_CALC_FOUND_ROWS", "FOUND_ROWS", "LAST_INSERT_ID",
                "GROUP_CONCAT", "CONCAT_WS", "FIELD", "FIND_IN_SET",
                "REGEXP", "RLIKE", "SOUNDS LIKE", "MATCH", "AGAINST", "IN BOOLEAN MODE",
                "IN NATURAL LANGUAGE MODE", "WITH QUERY EXPANSION",
                "LOCK TABLES", "UNLOCK TABLES", "FLUSH", "RESET", "PURGE",
                "LOAD DATA", "INFILE", "OUTFILE", "REPLACE", "INSERT IGNORE",
                "ON DUPLICATE KEY UPDATE", "LOW_PRIORITY", "HIGH_PRIORITY", "DELAYED"
            },
            ["PostgreSQL"] = new[] {
                "LIMIT", "OFFSET", "SERIAL", "BIGSERIAL", "SMALLSERIAL",
                "NOW", "CURRENT_TIMESTAMP", "CURRENT_DATE", "CURRENT_TIME", "LOCALTIME", "LOCALTIMESTAMP",
                "AGE", "DATE_PART", "DATE_TRUNC", "EXTRACT", "MAKE_DATE", "MAKE_TIME", "MAKE_TIMESTAMP",
                "TO_CHAR", "TO_DATE", "TO_TIMESTAMP", "TO_NUMBER",
                "COALESCE", "NULLIF", "GREATEST", "LEAST",
                "VARCHAR", "CHAR", "TEXT", "BYTEA", "UUID",
                "INTEGER", "INT", "SMALLINT", "BIGINT", "DECIMAL", "NUMERIC", "REAL", "DOUBLE PRECISION",
                "BOOLEAN", "BOOL", "DATE", "TIME", "TIMESTAMP", "TIMESTAMPTZ", "INTERVAL",
                "JSON", "JSONB", "ARRAY", "HSTORE", "CIDR", "INET", "MACADDR",
                "POINT", "LINE", "LSEG", "BOX", "PATH", "POLYGON", "CIRCLE",
                "TSVECTOR", "TSQUERY", "REGCLASS", "REGTYPE",
                "RETURNING", "ON CONFLICT", "DO UPDATE", "DO NOTHING", "EXCLUDED",
                "ILIKE", "SIMILAR TO", "POSIX", "~", "~*", "!~", "!~*",
                "EXPLAIN ANALYZE", "VACUUM", "ANALYZE", "REINDEX", "CLUSTER",
                "COPY", "LISTEN", "NOTIFY", "UNLISTEN",
                "LATERAL", "TABLESAMPLE", "BERNOULLI", "SYSTEM",
                "GENERATE_SERIES", "UNNEST", "ARRAY_AGG", "STRING_AGG", "JSON_AGG", "JSONB_AGG",
                "ROW_TO_JSON", "JSON_BUILD_OBJECT", "JSONB_BUILD_OBJECT",
                "ANY", "SOME", "ALL", "ARRAY_APPEND", "ARRAY_CAT", "ARRAY_REMOVE",
                "CROSSTAB", "TABLEFUNC", "EXTENSION", "CREATE EXTENSION"
            },
            ["SQLite"] = new[] {
                "LIMIT", "OFFSET", "AUTOINCREMENT", "ROWID", "OID", "_ROWID_",
                "DATETIME", "DATE", "TIME", "JULIANDAY", "STRFTIME",
                "IFNULL", "NULLIF", "COALESCE", "IIF", "UNLIKELY", "LIKELY",
                "INTEGER", "REAL", "TEXT", "BLOB", "NUMERIC", "BOOLEAN",
                "GLOB", "REGEXP", "MATCH",
                "ATTACH", "DETACH", "VACUUM", "ANALYZE", "REINDEX",
                "EXPLAIN", "EXPLAIN QUERY PLAN",
                "PRAGMA", "PRAGMA TABLE_INFO", "PRAGMA INDEX_LIST", "PRAGMA FOREIGN_KEY_LIST",
                "REPLACE", "INSERT OR REPLACE", "INSERT OR IGNORE", "INSERT OR ABORT",
                "INSERT OR ROLLBACK", "INSERT OR FAIL",
                "CONFLICT", "ABORT", "FAIL", "IGNORE", "ROLLBACK",
                "WITHOUT ROWID", "STRICT",
                "JSON_EXTRACT", "JSON_INSERT", "JSON_REPLACE", "JSON_SET", "JSON_REMOVE",
                "JSON_TYPE", "JSON_VALID", "JSON_QUOTE", "JSON_GROUP_ARRAY", "JSON_GROUP_OBJECT",
                "INSTR", "SUBSTR", "PRINTF", "TYPEOF", "ZEROBLOB", "RANDOMBLOB",
                "TOTAL", "GROUP_CONCAT", "ABS", "RANDOM", "HEX", "UNHEX", "QUOTE"
            },
            ["Oracle"] = new[] {
                "ROWNUM", "ROWID", "FETCH FIRST", "ROWS ONLY", "PERCENT",
                "SYSDATE", "SYSTIMESTAMP", "CURRENT_DATE", "CURRENT_TIMESTAMP", "LOCALTIMESTAMP",
                "ADD_MONTHS", "MONTHS_BETWEEN", "NEXT_DAY", "LAST_DAY", "TRUNC", "ROUND",
                "TO_CHAR", "TO_DATE", "TO_TIMESTAMP", "TO_NUMBER", "TO_CLOB", "TO_BLOB",
                "NVL", "NVL2", "DECODE", "COALESCE", "NULLIF", "LNNVL",
                "VARCHAR2", "NVARCHAR2", "CHAR", "NCHAR", "CLOB", "NCLOB", "BLOB", "BFILE",
                "NUMBER", "BINARY_FLOAT", "BINARY_DOUBLE", "INTEGER", "PLS_INTEGER",
                "DATE", "TIMESTAMP", "TIMESTAMP WITH TIME ZONE", "TIMESTAMP WITH LOCAL TIME ZONE",
                "INTERVAL YEAR TO MONTH", "INTERVAL DAY TO SECOND",
                "RAW", "LONG", "LONG RAW", "XMLTYPE", "SDO_GEOMETRY",
                "DUAL", "CONNECT BY", "START WITH", "PRIOR", "LEVEL", "SYS_CONNECT_BY_PATH",
                "CONNECT_BY_ROOT", "CONNECT_BY_ISLEAF", "CONNECT_BY_ISCYCLE",
                "LISTAGG", "WITHIN GROUP", "KEEP", "DENSE_RANK FIRST", "DENSE_RANK LAST",
                "MODEL", "DIMENSION BY", "MEASURES", "RULES",
                "PIVOT", "UNPIVOT", "CROSS APPLY", "OUTER APPLY",
                "FLASHBACK", "AS OF", "VERSIONS BETWEEN",
                "MERGE INTO", "WHEN MATCHED", "WHEN NOT MATCHED",
                "RETURNING INTO", "BULK COLLECT", "FORALL",
                "DBMS_OUTPUT", "PUT_LINE", "RAISE", "EXCEPTION", "PRAGMA",
                "REF CURSOR", "SYS_REFCURSOR", "TYPE", "RECORD", "VARRAY", "NESTED TABLE"
            },
            ["Standard SQL"] = new string[0]
        };

        private static readonly Dictionary<string, string[]> DialectMajorKeywords = new Dictionary<string, string[]>
        {
            ["SQL Server"] = new[] { "TOP", "OUTPUT", "MERGE", "CROSS APPLY", "OUTER APPLY", "PIVOT", "UNPIVOT", "OFFSET", "FETCH NEXT" },
            ["MySQL"] = new[] { "LIMIT", "STRAIGHT_JOIN", "REPLACE", "ON DUPLICATE KEY UPDATE" },
            ["PostgreSQL"] = new[] { "LIMIT", "RETURNING", "ON CONFLICT", "LATERAL" },
            ["SQLite"] = new[] { "LIMIT", "REPLACE" },
            ["Oracle"] = new[] { "FETCH FIRST", "CONNECT BY", "START WITH", "MERGE INTO", "PIVOT", "UNPIVOT" },
            ["Standard SQL"] = new string[0]
        };

        private string[] GetKeywordsForDialect()
        {
            var keywords = new List<string>(BaseSqlKeywords);
            if (DialectKeywords.TryGetValue(_selectedDialect, out var dialectSpecific))
            {
                keywords.AddRange(dialectSpecific);
            }
            return keywords.ToArray();
        }

        private string[] GetMajorKeywordsForDialect()
        {
            var keywords = new List<string>(BaseMajorKeywords);
            if (DialectMajorKeywords.TryGetValue(_selectedDialect, out var dialectSpecific))
            {
                keywords.AddRange(dialectSpecific);
            }
            return keywords.ToArray();
        }

        private string _inputSql = "";
        public string InputSql
        {
            get => _inputSql;
            set => SetProperty(ref _inputSql, value);
        }

        private string _outputSql = "";
        public string OutputSql
        {
            get => _outputSql;
            set => SetProperty(ref _outputSql, value);
        }

        private string _selectedDialect;
        public string SelectedDialect
        {
            get => _selectedDialect;
            set
            {
                if (SetProperty(ref _selectedDialect, value))
                {
                    SetSetting(nameof(SelectedDialect), value);
                    UpdateTemplatesForDialect();
                }
            }
        }

        private void UpdateTemplatesForDialect()
        {
            Templates = DialectTemplates.TryGetValue(_selectedDialect, out var templates)
                ? templates
                : DialectTemplates["Standard SQL"];
        }

        private int _indentSize;
        public int IndentSize
        {
            get => _indentSize;
            set
            {
                if (SetProperty(ref _indentSize, value))
                    SetSetting(nameof(IndentSize), value);
            }
        }

        private bool _uppercaseKeywords;
        public bool UppercaseKeywords
        {
            get => _uppercaseKeywords;
            set
            {
                if (SetProperty(ref _uppercaseKeywords, value))
                    SetSetting(nameof(UppercaseKeywords), value);
            }
        }

        private bool _newlineBeforeKeywords;
        public bool NewlineBeforeKeywords
        {
            get => _newlineBeforeKeywords;
            set
            {
                if (SetProperty(ref _newlineBeforeKeywords, value))
                    SetSetting(nameof(NewlineBeforeKeywords), value);
            }
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        private string _warnings;
        public string Warnings
        {
            get => _warnings;
            set => SetProperty(ref _warnings, value);
        }

        private string _optimizationTips;
        public string OptimizationTips
        {
            get => _optimizationTips;
            set => SetProperty(ref _optimizationTips, value);
        }

        private bool _isSqlReferenceExpanded;
        public bool IsSqlReferenceExpanded
        {
            get => _isSqlReferenceExpanded;
            set => SetProperty(ref _isSqlReferenceExpanded, value);
        }

        public List<string> Dialects { get; } = new List<string>
        {
            "SQL Server", "MySQL", "PostgreSQL", "SQLite", "Oracle", "Standard SQL"
        };

        public List<int> IndentSizes { get; } = new List<int> { 2, 4, 8 };

        private static readonly Dictionary<string, List<SqlTemplate>> DialectTemplates = new Dictionary<string, List<SqlTemplate>>
        {
            ["SQL Server"] = new List<SqlTemplate>
            {
                new SqlTemplate
                {
                    Name = "Basic SELECT",
                    Description = "Simple SELECT with WHERE",
                    Query = "SELECT TOP 10 id, name, email, created_at FROM users WHERE status = 'active' AND age > 18 ORDER BY created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "JOIN Query",
                    Description = "JOIN with multiple tables",
                    Query = "SELECT u.name, p.title, c.name AS category FROM users u INNER JOIN posts p ON u.id = p.user_id LEFT JOIN categories c ON p.category_id = c.id WHERE u.status = 'active' ORDER BY p.created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "Aggregation",
                    Description = "GROUP BY with HAVING",
                    Query = "SELECT u.name, COUNT(p.id) AS post_count, AVG(CAST(p.views AS FLOAT)) AS avg_views FROM users u LEFT JOIN posts p ON u.id = p.user_id GROUP BY u.id, u.name HAVING COUNT(p.id) > 5 ORDER BY avg_views DESC;"
                },
                new SqlTemplate
                {
                    Name = "INSERT",
                    Description = "INSERT with multiple values",
                    Query = "INSERT INTO users (name, email, status, created_at) VALUES ('John Doe', 'john@example.com', 'active', GETDATE()), ('Jane Smith', 'jane@example.com', 'active', GETDATE());"
                },
                new SqlTemplate
                {
                    Name = "UPDATE",
                    Description = "UPDATE with JOIN",
                    Query = "UPDATE u SET u.last_login = GETDATE(), up.login_count = up.login_count + 1 FROM users u INNER JOIN user_profiles up ON u.id = up.user_id WHERE u.email = 'user@example.com';"
                },
                new SqlTemplate
                {
                    Name = "CREATE TABLE",
                    Description = "CREATE with constraints",
                    Query = "CREATE TABLE posts (id INT IDENTITY(1,1) PRIMARY KEY, title NVARCHAR(255) NOT NULL, content NVARCHAR(MAX), user_id INT NOT NULL, status NVARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published')), created_at DATETIME2 DEFAULT GETDATE(), FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE);"
                }
            },
            ["MySQL"] = new List<SqlTemplate>
            {
                new SqlTemplate
                {
                    Name = "Basic SELECT",
                    Description = "Simple SELECT with WHERE",
                    Query = "SELECT id, name, email, created_at FROM users WHERE status = 'active' AND age > 18 ORDER BY created_at DESC LIMIT 10;"
                },
                new SqlTemplate
                {
                    Name = "JOIN Query",
                    Description = "JOIN with multiple tables",
                    Query = "SELECT u.name, p.title, c.name AS category FROM users u INNER JOIN posts p ON u.id = p.user_id LEFT JOIN categories c ON p.category_id = c.id WHERE u.status = 'active' ORDER BY p.created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "Aggregation",
                    Description = "GROUP BY with HAVING",
                    Query = "SELECT u.name, COUNT(p.id) AS post_count, AVG(p.views) AS avg_views FROM users u LEFT JOIN posts p ON u.id = p.user_id GROUP BY u.id, u.name HAVING COUNT(p.id) > 5 ORDER BY avg_views DESC;"
                },
                new SqlTemplate
                {
                    Name = "INSERT",
                    Description = "INSERT with multiple values",
                    Query = "INSERT INTO users (name, email, status, created_at) VALUES ('John Doe', 'john@example.com', 'active', NOW()), ('Jane Smith', 'jane@example.com', 'active', NOW());"
                },
                new SqlTemplate
                {
                    Name = "UPDATE",
                    Description = "UPDATE with JOIN",
                    Query = "UPDATE users u INNER JOIN user_profiles up ON u.id = up.user_id SET u.last_login = NOW(), up.login_count = up.login_count + 1 WHERE u.email = 'user@example.com';"
                },
                new SqlTemplate
                {
                    Name = "CREATE TABLE",
                    Description = "CREATE with constraints",
                    Query = "CREATE TABLE posts (id INT AUTO_INCREMENT PRIMARY KEY, title VARCHAR(255) NOT NULL, content TEXT, user_id INT NOT NULL, status ENUM('draft', 'published') DEFAULT 'draft', created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE);"
                }
            },
            ["PostgreSQL"] = new List<SqlTemplate>
            {
                new SqlTemplate
                {
                    Name = "Basic SELECT",
                    Description = "Simple SELECT with WHERE",
                    Query = "SELECT id, name, email, created_at FROM users WHERE status = 'active' AND age > 18 ORDER BY created_at DESC LIMIT 10;"
                },
                new SqlTemplate
                {
                    Name = "JOIN Query",
                    Description = "JOIN with multiple tables",
                    Query = "SELECT u.name, p.title, c.name AS category FROM users u INNER JOIN posts p ON u.id = p.user_id LEFT JOIN categories c ON p.category_id = c.id WHERE u.status = 'active' ORDER BY p.created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "Aggregation",
                    Description = "GROUP BY with HAVING",
                    Query = "SELECT u.name, COUNT(p.id) AS post_count, AVG(p.views) AS avg_views FROM users u LEFT JOIN posts p ON u.id = p.user_id GROUP BY u.id, u.name HAVING COUNT(p.id) > 5 ORDER BY avg_views DESC;"
                },
                new SqlTemplate
                {
                    Name = "INSERT",
                    Description = "INSERT with multiple values",
                    Query = "INSERT INTO users (name, email, status, created_at) VALUES ('John Doe', 'john@example.com', 'active', NOW()), ('Jane Smith', 'jane@example.com', 'active', NOW());"
                },
                new SqlTemplate
                {
                    Name = "UPDATE",
                    Description = "UPDATE with FROM",
                    Query = "UPDATE users SET last_login = NOW(), login_count = up.login_count + 1 FROM user_profiles up WHERE users.id = up.user_id AND users.email = 'user@example.com';"
                },
                new SqlTemplate
                {
                    Name = "CREATE TABLE",
                    Description = "CREATE with constraints",
                    Query = "CREATE TABLE posts (id SERIAL PRIMARY KEY, title VARCHAR(255) NOT NULL, content TEXT, user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE, status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published')), created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);"
                }
            },
            ["SQLite"] = new List<SqlTemplate>
            {
                new SqlTemplate
                {
                    Name = "Basic SELECT",
                    Description = "Simple SELECT with WHERE",
                    Query = "SELECT id, name, email, created_at FROM users WHERE status = 'active' AND age > 18 ORDER BY created_at DESC LIMIT 10;"
                },
                new SqlTemplate
                {
                    Name = "JOIN Query",
                    Description = "JOIN with multiple tables",
                    Query = "SELECT u.name, p.title, c.name AS category FROM users u INNER JOIN posts p ON u.id = p.user_id LEFT JOIN categories c ON p.category_id = c.id WHERE u.status = 'active' ORDER BY p.created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "Aggregation",
                    Description = "GROUP BY with HAVING",
                    Query = "SELECT u.name, COUNT(p.id) AS post_count, AVG(p.views) AS avg_views FROM users u LEFT JOIN posts p ON u.id = p.user_id GROUP BY u.id, u.name HAVING COUNT(p.id) > 5 ORDER BY avg_views DESC;"
                },
                new SqlTemplate
                {
                    Name = "INSERT",
                    Description = "INSERT with multiple values",
                    Query = "INSERT INTO users (name, email, status, created_at) VALUES ('John Doe', 'john@example.com', 'active', datetime('now')), ('Jane Smith', 'jane@example.com', 'active', datetime('now'));"
                },
                new SqlTemplate
                {
                    Name = "UPDATE",
                    Description = "UPDATE simple",
                    Query = "UPDATE users SET last_login = datetime('now') WHERE email = 'user@example.com';"
                },
                new SqlTemplate
                {
                    Name = "CREATE TABLE",
                    Description = "CREATE with constraints",
                    Query = "CREATE TABLE posts (id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL, content TEXT, user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE, status TEXT DEFAULT 'draft' CHECK (status IN ('draft', 'published')), created_at TEXT DEFAULT CURRENT_TIMESTAMP);"
                }
            },
            ["Oracle"] = new List<SqlTemplate>
            {
                new SqlTemplate
                {
                    Name = "Basic SELECT",
                    Description = "Simple SELECT with WHERE",
                    Query = "SELECT id, name, email, created_at FROM users WHERE status = 'active' AND age > 18 ORDER BY created_at DESC FETCH FIRST 10 ROWS ONLY;"
                },
                new SqlTemplate
                {
                    Name = "JOIN Query",
                    Description = "JOIN with multiple tables",
                    Query = "SELECT u.name, p.title, c.name AS category FROM users u INNER JOIN posts p ON u.id = p.user_id LEFT JOIN categories c ON p.category_id = c.id WHERE u.status = 'active' ORDER BY p.created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "Aggregation",
                    Description = "GROUP BY with HAVING",
                    Query = "SELECT u.name, COUNT(p.id) AS post_count, AVG(p.views) AS avg_views FROM users u LEFT JOIN posts p ON u.id = p.user_id GROUP BY u.id, u.name HAVING COUNT(p.id) > 5 ORDER BY avg_views DESC;"
                },
                new SqlTemplate
                {
                    Name = "INSERT",
                    Description = "INSERT with multiple values",
                    Query = "INSERT ALL INTO users (name, email, status, created_at) VALUES ('John Doe', 'john@example.com', 'active', SYSDATE) INTO users (name, email, status, created_at) VALUES ('Jane Smith', 'jane@example.com', 'active', SYSDATE) SELECT 1 FROM DUAL;"
                },
                new SqlTemplate
                {
                    Name = "UPDATE",
                    Description = "UPDATE with subquery",
                    Query = "UPDATE users u SET u.last_login = SYSDATE, u.login_count = (SELECT up.login_count + 1 FROM user_profiles up WHERE up.user_id = u.id) WHERE u.email = 'user@example.com';"
                },
                new SqlTemplate
                {
                    Name = "CREATE TABLE",
                    Description = "CREATE with constraints",
                    Query = "CREATE TABLE posts (id NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, title VARCHAR2(255) NOT NULL, content CLOB, user_id NUMBER NOT NULL REFERENCES users(id) ON DELETE CASCADE, status VARCHAR2(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published')), created_at TIMESTAMP DEFAULT SYSTIMESTAMP);"
                }
            },
            ["Standard SQL"] = new List<SqlTemplate>
            {
                new SqlTemplate
                {
                    Name = "Basic SELECT",
                    Description = "Simple SELECT with WHERE",
                    Query = "SELECT id, name, email, created_at FROM users WHERE status = 'active' AND age > 18 ORDER BY created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "JOIN Query",
                    Description = "JOIN with multiple tables",
                    Query = "SELECT u.name, p.title, c.name AS category FROM users u INNER JOIN posts p ON u.id = p.user_id LEFT JOIN categories c ON p.category_id = c.id WHERE u.status = 'active' ORDER BY p.created_at DESC;"
                },
                new SqlTemplate
                {
                    Name = "Aggregation",
                    Description = "GROUP BY with HAVING",
                    Query = "SELECT u.name, COUNT(p.id) AS post_count, AVG(p.views) AS avg_views FROM users u LEFT JOIN posts p ON u.id = p.user_id GROUP BY u.id, u.name HAVING COUNT(p.id) > 5 ORDER BY avg_views DESC;"
                },
                new SqlTemplate
                {
                    Name = "INSERT",
                    Description = "INSERT with multiple values",
                    Query = "INSERT INTO users (name, email, status, created_at) VALUES ('John Doe', 'john@example.com', 'active', CURRENT_TIMESTAMP), ('Jane Smith', 'jane@example.com', 'active', CURRENT_TIMESTAMP);"
                },
                new SqlTemplate
                {
                    Name = "UPDATE",
                    Description = "UPDATE simple",
                    Query = "UPDATE users SET last_login = CURRENT_TIMESTAMP WHERE email = 'user@example.com';"
                },
                new SqlTemplate
                {
                    Name = "CREATE TABLE",
                    Description = "CREATE with constraints",
                    Query = "CREATE TABLE posts (id INT PRIMARY KEY, title VARCHAR(255) NOT NULL, content VARCHAR(4000), user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE, status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published')), created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);"
                }
            }
        };

        private List<SqlTemplate> _templates;
        public List<SqlTemplate> Templates
        {
            get => _templates;
            private set => SetProperty(ref _templates, value);
        }

        public ICommand FormatCommand { get; }
        public ICommand MinifyCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LoadTemplateCommand { get; }
        public ICommand ToggleSqlReferenceCommand { get; }
        public ICommand CopyInputCommand { get; }
        public ICommand CopyOutputCommand { get; }

        public Action<string> CopyToClipboardAction { get; set; }

        public SqlFormatterToolViewModel()
        {
            LoadSettings();
            UpdateTemplatesForDialect();

            FormatCommand = new RelayCommand(Format);
            MinifyCommand = new RelayCommand(Minify);
            ClearCommand = new RelayCommand(Clear);
            LoadTemplateCommand = new RelayCommand<SqlTemplate>(LoadTemplate);
            ToggleSqlReferenceCommand = new RelayCommand(() => IsSqlReferenceExpanded = !IsSqlReferenceExpanded);
            CopyInputCommand = new RelayCommand(() => CopyToClipboardAction?.Invoke(InputSql ?? ""));
            CopyOutputCommand = new RelayCommand(() => CopyToClipboardAction?.Invoke(OutputSql ?? ""));
        }

        private void LoadSettings()
        {
            _selectedDialect = GetSetting(nameof(SelectedDialect), "SQL Server");
            _indentSize = GetSetting(nameof(IndentSize), 2);
            _uppercaseKeywords = GetSetting(nameof(UppercaseKeywords), true);
            _newlineBeforeKeywords = GetSetting(nameof(NewlineBeforeKeywords), true);
        }

        private void Format()
        {
            if (string.IsNullOrWhiteSpace(InputSql))
            {
                Error = "Please enter a SQL query.";
                OutputSql = "";
                return;
            }

            Error = null;
            Warnings = null;

            try
            {
                if (SelectedDialect == "SQL Server")
                {
                    FormatSqlServer();
                }
                else
                {
                    FormatGeneric();
                }
                GenerateOptimizationTips();
            }
            catch (Exception ex)
            {
                Error = $"Error formatting SQL: {ex.Message}";
            }
        }

        private void FormatSqlServer()
        {
            var parser = new TSql130Parser(false);
            IList<ParseError> errors;

            using (var reader = new StringReader(InputSql))
            {
                var fragment = parser.Parse(reader, out errors);

                if (errors.Count > 0)
                {
                    var errorMessages = errors.Select(e => $"Line {e.Line}, Col {e.Column}: {e.Message}");
                    Error = string.Join("\n", errorMessages);
                    OutputSql = "";
                    return;
                }

                var generator = new Sql130ScriptGenerator(new SqlScriptGeneratorOptions
                {
                    KeywordCasing = UppercaseKeywords ? KeywordCasing.Uppercase : KeywordCasing.Lowercase,
                    IndentationSize = IndentSize,
                    SqlEngineType = SqlEngineType.All,
                    IncludeSemicolons = true,
                    NewLineBeforeFromClause = NewlineBeforeKeywords,
                    NewLineBeforeWhereClause = NewlineBeforeKeywords,
                    NewLineBeforeGroupByClause = NewlineBeforeKeywords,
                    NewLineBeforeHavingClause = NewlineBeforeKeywords,
                    NewLineBeforeOrderByClause = NewlineBeforeKeywords,
                    NewLineBeforeJoinClause = NewlineBeforeKeywords,
                    NewLineBeforeOutputClause = NewlineBeforeKeywords,
                    NewLineBeforeOffsetClause = NewlineBeforeKeywords,
                    NewLineBeforeOpenParenthesisInMultilineList = true,
                    NewLineBeforeCloseParenthesisInMultilineList = true,
                    AlignClauseBodies = true,
                    AlignColumnDefinitionFields = true,
                    AlignSetClauseItem = true,
                    AsKeywordOnOwnLine = false,
                    MultilineInsertSourcesList = true,
                    MultilineInsertTargetsList = true,
                    MultilineSelectElementsList = true,
                    MultilineSetClauseItems = true,
                    MultilineViewColumnsList = true,
                    MultilineWherePredicatesList = true,
                    IndentViewBody = true
                });

                generator.GenerateScript(fragment, out var formattedSql);
                OutputSql = formattedSql;
            }
        }

        private void FormatGeneric()
        {
            Validate();

            var formatted = InputSql;
            var keywords = GetKeywordsForDialect();
            var majorKeywords = GetMajorKeywordsForDialect();

            // Normalize whitespace
            formatted = Regex.Replace(formatted, @"\s+", " ").Trim();

            // Apply keyword casing
            if (UppercaseKeywords)
            {
                foreach (var keyword in keywords.OrderByDescending(k => k.Length))
                {
                    var pattern = $@"\b{Regex.Escape(keyword)}\b";
                    formatted = Regex.Replace(formatted, pattern, keyword.ToUpperInvariant(), RegexOptions.IgnoreCase);
                }
            }

            // Add newlines before major keywords
            if (NewlineBeforeKeywords)
            {
                foreach (var keyword in majorKeywords.OrderByDescending(k => k.Length))
                {
                    var kw = UppercaseKeywords ? keyword.ToUpperInvariant() : keyword;
                    var pattern = $@"\s+({Regex.Escape(kw)})\b";
                    formatted = Regex.Replace(formatted, pattern, $"\n{kw}", RegexOptions.IgnoreCase);
                }
            }

            // Handle indentation for subqueries
            var lines = formatted.Split('\n');
            var result = new StringBuilder();
            int indentLevel = 0;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Decrease indent for closing parentheses
                if (trimmed.StartsWith(")"))
                    indentLevel = Math.Max(0, indentLevel - 1);

                result.AppendLine(new string(' ', indentLevel * IndentSize) + trimmed);

                // Increase indent for opening parentheses without closing
                int opens = trimmed.Count(c => c == '(');
                int closes = trimmed.Count(c => c == ')');
                indentLevel += opens - closes;
                indentLevel = Math.Max(0, indentLevel);
            }

            OutputSql = result.ToString().Trim();
        }

        private void Minify()
        {
            if (string.IsNullOrWhiteSpace(InputSql))
            {
                Error = "Please enter a SQL query.";
                OutputSql = "";
                return;
            }

            Error = null;
            Warnings = null;

            try
            {
                var minified = Regex.Replace(InputSql, @"\s+", " ");
                minified = Regex.Replace(minified, @"\s*([(),;])\s*", "$1");
                minified = Regex.Replace(minified, @"\s*(=|<|>|<=|>=|!=|<>)\s*", "$1");
                OutputSql = minified.Trim();
            }
            catch (Exception ex)
            {
                Error = $"Error minifying SQL: {ex.Message}";
            }
        }

        private void Validate()
        {
            var warnings = new List<string>();

            // Check for unmatched parentheses
            int opens = InputSql.Count(c => c == '(');
            int closes = InputSql.Count(c => c == ')');
            if (opens != closes)
                warnings.Add("Unmatched parentheses detected.");

            // Check for potential SQL injection patterns
            if (Regex.IsMatch(InputSql, @"['""][^'""]*['""][\s;]*(-{2}|/\*)", RegexOptions.IgnoreCase))
                warnings.Add("Potential SQL injection pattern detected.");

            Warnings = warnings.Count > 0 ? string.Join("\n", warnings) : null;
        }

        private void GenerateOptimizationTips()
        {
            var tips = new List<string>();
            var upperSql = InputSql.ToUpperInvariant();

            if (upperSql.Contains("SELECT *"))
                tips.Add("Avoid SELECT * - specify only needed columns for better performance.");

            if (Regex.IsMatch(upperSql, @"LIKE\s+'%[^%]+%'"))
                tips.Add("Leading wildcards in LIKE queries can't use indexes - consider full-text search.");

            if (upperSql.Contains("ORDER BY") && !upperSql.Contains("LIMIT"))
                tips.Add("ORDER BY without LIMIT can be expensive on large datasets.");

            if (Regex.IsMatch(upperSql, @"WHERE.*OR.*OR"))
                tips.Add("Multiple OR conditions might benefit from UNION or restructuring.");

            if (!upperSql.Contains("INDEX") && upperSql.Contains("WHERE"))
                tips.Add("Consider adding indexes on columns used in WHERE clauses.");

            OptimizationTips = tips.Count > 0 ? string.Join("\n", tips) : null;
        }

        private void Clear()
        {
            InputSql = "";
            OutputSql = "";
            Error = null;
            Warnings = null;
            OptimizationTips = null;
        }

        private void LoadTemplate(SqlTemplate template)
        {
            if (template != null)
            {
                InputSql = template.Query;
                Error = null;
                Warnings = null;
                OptimizationTips = null;
            }
        }
    }
}
