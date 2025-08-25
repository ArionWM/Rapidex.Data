using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.SqlServer;

internal class DbSqlServerExceptionTranslator : IExceptionTranslator
{
    public Exception Translate(Exception ex)
    {
        if(ex is TranslatedException)
        {
            return ex;
        }

        SqlException sqlException = ex as SqlException;
        if (sqlException == null)
        {
            return null;
        }

        //https://learn.microsoft.com/en-us/previous-versions/sql/sql-server-2008-r2/cc645603(v=sql.105)?redirectedfrom=MSDN

        switch (sqlException.Number)
        {
            case 233:
            case 4060:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "Veritabanına bağlantı sağlanamadı. SQL bağlantı metnindeki hedef adres ve veritabanı adını kontrol edin", ex);
            case 18456:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "Verilen kullanıcı adı / parola bilgileri ile SQL Sunucusuna oturum açılamıyor", ex);
            case 547:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Foreign key violation.", ex);
            case 2627:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Violation of primary key constraint.", ex);
            case 2601:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Violation of unique index constraint.", ex);
            case 53:
                return new TranslatedException(ExceptionTarget.ITDepartment_Infrastructure, "Network-related or instance-specific error.", ex);
            case 2:
                return new TranslatedException(ExceptionTarget.ITDepartment_Infrastructure, "SQL Server bağlantısı zaman aşımına uğradı. Bağlantı bilgilerinin doğruluğunu ya da SQL Server'ın çalışır durumda olduğunu kontrol edin.", ex);
            case 1205:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Deadlock victim.", ex);
            case 229:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "SQL kullanıcısının bu SQL veritabanı nesnesini sorgulamak için yetkisi yok. SQL Server'dan sorumlu ekibe bu hatanın tüm bilgileri ile haber verin.", ex);
            case 207:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, $"Açıklamada belirtilen alan adı veritabanı / tabloda bulunmuyor: \r\n{ex.Message}", ex);
            case 208:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Hedeflenen tablo vs (açıklamanın içinde) bulunamadı", ex);
            case 4064:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "SQL server bağlantı metninde yer alan (hedeflenen) veritabanına oturum açılamıyor. Bağlantı metninde verilen kullanıcının yetkisi olmayabilir, veritabanı adı yanlış ya da veritabanı kapalı olabilir ", ex);
            case 18452:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "SQL server bağlantı metninde yer alan kullanıcı ile oturum açılamıyor. Bağlandığınız sistem SQL Server için güvenli işaretlenmemiş. SQL Server'dan sorumlu ekibe bu hatanın tüm bilgileri ile haber verin.", ex);
            case 11001:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "A network-related or instance-specific error occurred while establishing a connection to SQL Server.", ex);
            case 10054:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "A transport-level error has occurred when sending the request to the server.", ex);
            case 10060:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "A network-related or instance-specific error occurred while establishing a connection to SQL Server.", ex);
            case 121:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "The semaphore timeout period has expired.", ex);
            case 64:
                return new TranslatedException(ExceptionTarget.ITDepartment_ApplicationManagement, "A connection was successfully established with the server, but then an error occurred during the login process.", ex);
            case 2714:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "There is already an object named in the database.", ex);
            case 3728:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "'name' is not a constraint.", ex);
            case 3727:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Could not drop constraint. See previous errors.", ex);
            case 15151:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "Cannot drop the login because it does not exist or you do not have permission.", ex);
            case 15335:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "The database principal owns a schema in the database, and cannot be dropped.", ex);
            case 2715:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, $"Unsupported or wrong data type: {ex.Message}", ex);
            default:
                return new TranslatedException(ExceptionTarget.ApplicationSupport, "An unknown SQL error occurred.", ex);

        }

        /*
        1.	4060 - Cannot open database requested by the login. The login failed.
        2.	18456 - Login failed for user.
        3.	547 - Foreign key violation.
        4.	2627 - Violation of primary key constraint.
        5.	2601 - Violation of unique index constraint.
        6.	53 - Network-related or instance-specific error.
        7.	2 - Connection timeout.
        8.	1205 - Deadlock victim.
        9.	229 - The SELECT permission was denied on the object.
        10.	233 - The client was unable to establish a connection.
        11.	207 - Invalid column name.
        12.	208 - Invalid object name.
        13.	4064 - Cannot open user default database. Login failed.
        14.	18452 - Login failed. The login is from an untrusted domain.
        15.	11001 - A network-related or instance-specific error occurred while establishing a connection to SQL Server.
        16.	10054 - A transport-level error has occurred when sending the request to the server.
        17.	10060 - A network-related or instance-specific error occurred while establishing a connection to SQL Server.
        18.	121 - The semaphore timeout period has expired.
        19.	64 - A connection was successfully established with the server, but then an error occurred during the login process.
        20.	4060 - Cannot open database requested by the login. The login failed.

        1.	18456 - Login failed for user.
        2.	4060 - Cannot open database requested by the login. The login failed.
        3.	229 - The SELECT permission was denied on the object.
        4.	4064 - Cannot open user default database. Login failed.
        5.	18452 - Login failed. The login is from an untrusted domain.

1.	547 - Foreign key violation.
2.	2627 - Violation of primary key constraint.
3.	2601 - Violation of unique index constraint.
4.	1205 - Deadlock victim.
5.	207 - Invalid column name.
6.	208 - Invalid object name.

1.	2714 - There is already an object named in the database.
2.	3728 - 'name' is not a constraint.
3.	3727 - Could not drop constraint. See previous errors.
4.	15151 - Cannot drop the login because it does not exist or you do not have permission.
5.	15335 - Error: The database principal owns a schema in the database, and cannot be dropped.
                     */
    }
}
