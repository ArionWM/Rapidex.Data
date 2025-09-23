# Veri Türleri

Metada içerisinde ifade edilen adları ile türler aşağıda

İfade şekli : `Tür Adı` (`Alternatif adı`)

Bkz: [EntityMetadata.Task.json](/Rapidex.Data/Documents/SampleData/EntityMetadata.Task.json)


## string

250 karakterlik metin alanı 

||||
|---|---|
|Tür: JSON|string||
|Tür: C#|string||
|Tür: SQL|nvarchar(250)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|250||
|Lazy|hayır||

## byte

8 bitlik tam sayı alanı 

## short

16 bitlik tam sayı alanı 


## int (int32)

32 bitlik tam sayı alanı 


## long (int64)

64 bitlik tam sayı alanı


## double (float)

64 bitlik ondalıklı sayı alanı


## decimal

128 bitlik ondalıklı sayı alanı


## datetime

Tarih ve saat alanı

## date

Sadece tarih alanı


## time

Sadece saat alanı


## timespan (datetimediff)

Zaman farkı alanı. Int64 tutulur.

(Henüz yazılmadı)

## guid

GUID alanı


## bool (boolean, yesno)

Mantıksal değer alanı

||||
|---|---|
|Tür: JSON|bool, boolean, yesno||
|Tür: C#|bool||
|Tür: SQL|bit||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|---||
|Lazy|hayır||


## binary (byte[])

Binary / blob veri alanı. 

||||
|---|---|
|Tür: JSON|binary, byte[]||
|Tür: C#|byte[]||
|Tür: SQL|image||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|sonsuz||
|Lazy|hayır||


## xml

(Henüz yazılmadı)


## currency

Bkz: Currency.cs

Para birimi alanı, bu tanım `ilgili alan adı` + `Currency` adında bir metin alanını da beraberinde ekler.

||||
|---|---|
|Tür: JSON|currency||
|Tür: C#|Currency||
|Tür: SQL|decimal||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|24,8||
|Lazy|hayır||
|Ek alan|evet|`ilgili alan adı` + `Currency` adında bir metin alanı ekler. Bu alan para birimi kodunu tutar (ISO 4217)|


## percentage

100'lük skalada yüzde bilgisi tutar.

||||
|---|---|
|Tür: JSON|percent||
|Tür: C#|Percent||
|Tür: SQL|short||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|2||
|Lazy|hayır||



## phone

Bkz: Phone.cs

||||
|---|---|
|Tür: JSON|phone||
|Tür: C#|Phone||
|Tür: SQL|nvarchar(20)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|20||
|Lazy|hayır||


## email

Bkz: EMail.cs

||||
|---|---|
|Tür: JSON|email||
|Tür: C#|email||
|Tür: SQL|nvarchar(200)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|200||
|Lazy|hayır||

## url

(Henüz yazılmadı)


## color

Hex ya da color name türünden renk bilgisi tutar. UI'da ayırt edilebilmesi ve özel bileşen (Color Picker) tanımlanabilmesi için özel tanımlıdır.

||||
|---|---|
|Tür: JSON|color||
|Tür: C#|Color||
|Tür: SQL|nvarchar(20)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|20||
|Lazy|hayır||




## datetimeStartEnd

||||
|---|---|
|Tür: JSON|datetimeStartEnd||
|Tür: C#|DatetimeStartEnd||
|Tür: SQL|--- (sanal alan)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Uzunluk|---||
|Lazy|hayır||
|Ek alan|evet|`ilgili alan adı` + `Start` ve `ilgili alan adı` + `End` adında iki tarih alanı ekler.|

## image

Image binary türünde "blob" veri alanıdır. 

||||
|---|---|
|Tür: JSON|image||
|Tür: C#|Image||
|Tür: SQL|image||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|evet||

### Lazy

Yüklenen entity'ler içerisinde değeri yüklenmez. 

* Concrete tanımlar için: Ayrıca `GetContent()` metotu ile içeriği çağırılır.
* Json ya da dinamik tanımlar için: (Henüz yazılmadı)

## reference

1eN referans alanıdır. 

||||
|---|---|
|Tür: C#|Reference<>||
|Tür: JSON|reference|`reference` alanında hedeflenen entity name belirtilmelidir|
|Tür: SQL|int64||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|evet||

### Lazy

Yüklenen entity'ler içerisinde ID değeri atanır. 

* Concrete tanımlar için: Implicit conversion mevcut. Ayrıca `GetContent()` metotu ile içeriği çağırılır.
* Json ya da dinamik tanımlar için: (Henüz yazılmadı)

### Şemalar arası kullanım (Cross Schema)

(Henüz yazılmadı)



## tags

||||
|---|---|
|Tür: C#|Tags||
|Tür: JSON|tags||
|Tür: SQL|string||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|hayır||

Sadece "HasTags" behavior'ı ile kullanılır. Doğrudan kullanılması açısından desteği yoktur.


## text

string türünde ancak sonsuz uzunlukta metin alanıdır (blob).

||||
|---|---|
|Tür: C#|Text||
|Tür: JSON|text||
|Tür: SQL|nvarchar(max)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|hayır||

## json

string türünde ancak sonsuz uzunlukta metin alanıdır. Pratikte arayüz için Text'ten farkı yoktur (şimdilik)

||||
|---|---|
|Tür: C#|Json||
|Tür: JSON|json||
|Tür: SQL|nvarchar(max)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|hayır||



## richText

(Henüz yazılmadı)

string türünde ancak sonsuz uzunlukta metin alanıdır (blob). Zengin metin içeriklerini tutar.

||||
|---|---|
|Tür: C#|RichText||
|Tür: JSON|richText||
|Tür: SQL|nvarchar(max)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|hayır||
|Ek alan|evet|`ilgili alan adı` + `Type` adında bir metin alanı ekler. Bu alan metin formatı türünü tutar (html, markdown)|

## password

İki yönlü (şifrele / çöz) şifrelenmiş bilgi saklama türüdür

||||
|---|---|
|Tür: C#|Password||
|Tür: JSON|password||
|Tür: SQL|nvarchar(200)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|hayır||

### Açıklama

Password nesnesinde `Value`özelliğinde şifrelenmiş bilgi taşınır (prematüre entity'ler hariç) şifrenin çözülmesi için `Password.Decrypt()` metodu kullanılır

Password alanı içerisindeki veriyi JsonSerileştirmesinde dışarıya sunmaz. Serileştirmelerde dışarıya '*****' metni verilir.

Password alanı, içeriğindeki veriyi, sahibi olan entity'nin şema adı ve id'sini kullanarak şifreler. Bir entity'deki şifrelenmiş bir bilgi, farklı şema / id'deki bir diğer entity'ye kopyalandığında çözülemeyecektir.

## oneWayPassword

Tek yönlü (şifrele / eldeki ile karşılaştır) şifrelenmiş bilgi saklama türüdür. Geri dönüştürülemez.

İçerdiği bilginin kontrolü için `OneWayPassword.IsEqual()` metodu kullanılır.

OneWayPassword alanı içerisindeki veriyi JsonSerileştirmesinde dışarıya sunmaz. Serileştirmelerde dışarıya '*****' metni verilir.


||||
|---|---|
|Tür: C#|OneWayPassword||
|Tür: JSON|oneWayPassword||
|Tür: SQL|nvarchar(200)||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|hayır||

### Açıklama

Password nesnesinde `Value`özelliğinde şifrelenmiş bilgi taşınır (prematüre entity'ler hariç) şifrenin çözülmesi için `Password.Decrypt()` metodu kullanılır

Password alanı içerisindeki veriyi JsonSerileştirmesinde dışarıya sunmaz. Serileştirmelerde dışarıya '*****' metni verilir.

Password alanı, içeriğindeki veriyi, sahibi olan entity'nin şema adı ve id'sini kullanarak şifreler. Bir entity'deki şifrelenmiş bir bilgi, farklı şema / id'deki bir diğer entity'ye kopyalandığında çözülemeyecektir.



## relationOne2N

||||
|---|---|
|Tür: C#|RelationOne2N\<DetailEntity\>||
|Tür: JSON|relationOne2N|`reference` alanında hedeflenen entity name belirtilmelidir|
|Tür: SQL|int64||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|evet||

### Lazy

* Concrete tanımlar için: `DetailEntity[]`'e Implicit conversion mevcut. Ayrıca `GetContent()` metotu ile içeriği çağırılır.
* Json ya da dinamik tanımlar için: (Henüz yazılmadı)

### Açıklama

`DetailEntity` içerisinde `Parent<ParentEntityName>` (Örn `ParentInvoice`) alanı ile master kayıta referans verilir.




## relationN2N

(Henüz yazılmadı)

||||
|---|---|
|Tür: C#|RelationN2N\<OtherEntity\>|Çift taraflı çalışır (OtherEntity de de tersi görünür)|
|Tür: JSON|relationN2N|`reference` alanında hedeflenen entity name belirtilmelidir|
|Tür: SQL|int64||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|evet||

### Lazy

* Concrete tanımlar için: `OtherEntity[]`'e Implicit conversion mevcut. Ayrıca `GetContent()` metotu ile içeriği çağırılır.
* Json ya da dinamik tanımlar için: (Henüz yazılmadı)

### Açıklama

N2N ilişkiyi sağlayacak bir tablo üretilir (Henüz detayları belli değil).


## enumeration

(Henüz yazılmadı)

||||
|---|---|
|Tür: C#|Enumeration\<T\>| T: c# enumeration'u |
|Tür: JSON|enum|`reference` alanında enum değerlerini tutan entity name verilir|
|Tür: SQL|int32||
|Tür: SQLite|---||
|Tür: PosgreSQL|---||
|Lazy|hayır||

### Açıklama

Enum değerleri enum adı ile verilen (c# için tanımlanan enum adı, json için henüz yazılmadı) entity ile veritabanında tutulur.

#### c#
Enum ilk değerleri için normal enumeration tanımı yapılır. 

#### JSON

(henüz yazılmadı)

#### Değerlerin arayüze aktarılması ve yönetimi (controllers)

(henüz yazılmadı)



## files

(Henüz yazılmadı)

`relationOne2N<FileRecord>` tanımını yapar

## notes

(Henüz yazılmadı)

`relationOne2N<NoteRecord>` tanımını yapar
