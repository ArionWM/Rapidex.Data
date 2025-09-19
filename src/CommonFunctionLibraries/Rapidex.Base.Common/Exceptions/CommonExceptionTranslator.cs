using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rapidex
{
    internal class CommonExceptionTranslator : IExceptionTranslator
    {
        public Exception Translate(Exception ex, string? additionalInfo = null)
        {
            if (ex is TranslatedException)
            {
                return ex;
            }

            switch (ex)
            {
                case NotImplementedException notImplementedException:
                    return new TranslatedException(ExceptionTarget.DevelopmentTeam, "Henüz tamamlanmamış, geliştirme ekibine başvurun" + (additionalInfo.IsNullOrEmpty() ? "" : "\r\n" + additionalInfo), ex);

                case NotSupportedException notSupportedException:
                    return new TranslatedException(ExceptionTarget.DevelopmentTeam, "Bu işlem desteklenmiyor, bu hata mesajında aşağıda geçen metinlerin tümü ile geliştirme ekibine başvurun" + (additionalInfo.IsNullOrEmpty() ? "" : "\r\n" + additionalInfo), ex);

                case FileNotFoundException fileNotFoundException:
                    return new TranslatedException(ExceptionTarget.ApplicationSupport, "Dosya bulunamadı (açıklamalara bakınız)" + (additionalInfo.IsNullOrEmpty() ? "" : "\r\n" + additionalInfo), ex);

                case DirectoryNotFoundException directoryNotFoundException:
                    return new TranslatedException(ExceptionTarget.DevelopmentTeam, "Klasör bulunamadı (açıklamalara bakınız)" + (additionalInfo.IsNullOrEmpty() ? "" : "\r\n" + additionalInfo), ex);
            }

            return null;

        }
    }
}
