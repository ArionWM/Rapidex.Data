using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTests;
public interface ICoreTestFixture //From ProCore
{
    void Init();

    /// <summary>
    /// Sadece ön bellekler temizlenir
    /// </summary>
    void ClearCaches();

    /// <summary>
    /// Tüm test yapısını kaldırır
    /// Örn: Veritabanları silinir vs.
    /// </summary>
    void Reinit();

    /// <summary>
    /// Test için kullanılan içerik dosyalarını okur ve string olarak döner. 
    /// </summary>
    /// <param name="relativeFilePath"></param>
    /// <returns></returns>
    string GetFileContentAsString(string relativeFilePath);

}
