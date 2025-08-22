using Microsoft.Extensions.Hosting;
using MimeTypes;

using Rapidex.Data.Entities;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data;

public class DataTypeTests : DbDependedTestsBase<DbSqlServerProvider>
{
    //Currency with types
    //Reference with types
    //type conversions (see: BasicBaseDataTypeDescriptionProvider)

    public DataTypeTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Image_01_BasicSaveAndLoad()
    {
        var db = Database.Scopes.Db();

        db.ReAddReCreate<ConcreteEntity01>();


        byte[] imageContentOriginal01 = this.Fixture.GetFileContentAsBinary("TestContent\\Image01.png");
        int hash01 = HashHelper.GetStableHashCode(imageContentOriginal01);

        ConcreteEntity01 entity = db.New<ConcreteEntity01>();
        entity.Name = "Binary 001";

        entity.Picture.Set(imageContentOriginal01, "image.png", MimeTypeMap.GetMimeType("png"));
        entity.Save();

        await db.CommitOrApplyChanges();

        long blobId01_check = entity.Picture.TargetId;

        db.Metadata.ReAdd<ConcreteEntity01>();

        ConcreteEntity01 ent01 = await db.GetQuery<ConcreteEntity01>().First();

        long blobId01 = ent01.Picture.TargetId;

        byte[] imageContent01 = ent01.Picture;
        Assert.NotNull(imageContent01);

        int hash02_check = HashHelper.GetStableHashCode(imageContent01);
        Assert.Equal(hash01, hash02_check);


        byte[] imageContentOriginal02 = this.Fixture.GetFileContentAsBinary("TestContent\\Image02.png");
        int hash02 = HashHelper.GetStableHashCode(imageContentOriginal02);

        entity.Picture.Set(imageContentOriginal02, "image.png", MimeTypeMap.GetMimeType("png"));
        entity.Save();

        await db.CommitOrApplyChanges();

        //Yeni bir içerik yüklendiğinde Id değişmemeli, önceki blobRecord kaydı güncellenmeli
        long blobId02_check = ent01.Picture.TargetId;
        Assert.Equal(blobId01_check, blobId02_check);
    }

    [Fact]
    public async Task Image_02_NullContentDontCreateOrDeleteBlobRecord()
    {
        this.Fixture.ClearCaches();

        var db = Database.Scopes.Db();

        db.ReAddReCreate<BlobRecord>();
        db.ReAddReCreate<ConcreteEntity01>();

        byte[] imageContentOriginal01 = this.Fixture.GetFileContentAsBinary("TestContent\\Image01.png");
        int hash01 = HashHelper.GetStableHashCode(imageContentOriginal01);

        ConcreteEntity01 entity = db.New<ConcreteEntity01>();
        entity.Name = "Binary 001";
        entity.Picture.Set(imageContentOriginal01, "image.png", MimeTypeMap.GetMimeType("png"));
        entity.Save();

        await db.CommitOrApplyChanges();

        long entityId01 = entity.Id;

        long blobRecId01 = entity.Picture.TargetId;
        BlobRecord br = await db.Find<BlobRecord>(blobRecId01);
        Assert.NotNull(br);

        int hash01_check = HashHelper.GetStableHashCode(br.Data);
        Assert.Equal(hash01, hash01_check);

        entity = await db.Find<ConcreteEntity01>(entityId01);
        entity.Picture.SetEmpty();
        entity.Save();

        await db.CommitOrApplyChanges();

        Assert.Equal(DatabaseConstants.DEFAULT_EMPTY_ID, entity.Picture.TargetId);

        br = await db.Find<BlobRecord>(blobRecId01);
        Assert.Null(br);



    }


    [Fact]
    public virtual async Task Enum_02_Assignment()
    {
        var db = Database.Scopes.Db();

        db.ReAddReCreate<ConcreteEntity01>();

        ConcreteEntity01 ent01 = db.New<ConcreteEntity01>();
        ent01.ContactType = ContactType.Corporate;
        ent01.Save();



        ConcreteEntity01 ent02 = db.New<ConcreteEntity01>();
        ent02["ContactType"] = ContactType.Corporate; //Bu durumda farklı bir atama yapısı çalışıyor
        ent02.Save();
        await db.CommitOrApplyChanges();

        long id01 = ent01.Id;
        long id02 = ent02.Id;

        //this.Fixture.ClearCaches();

        //TODO: Clear entity cache

        db = Database.Scopes.Db();
        db.Metadata.ReAdd<ConcreteEntity01>();

        ConcreteEntity01 ent01_check = await db.Find<ConcreteEntity01>(id01);
        Assert.Equal(ContactType.Corporate, (ContactType)ent01_check.ContactType);

        ConcreteEntity01 ent02_check = await db.Find<ConcreteEntity01>(id02);
        Assert.Equal(ContactType.Corporate, (ContactType)ent02_check.ContactType);

    }

    [Fact]
    public virtual async Task Enum_03_PredefinedValues()
    {
        var db = Database.Scopes.Db();

        db.ReAddReCreate<ConcreteEntity01>();
        //Database.Metadata.AddIfNotExist<BlobRecord>();
        db.Metadata.Remove("ContactType");

        var emCT = db.Metadata.AddEnum<ContactType>();
        db.Structure.DropEntity("ContactType");
        db.Structure.ApplyEntityStructure(emCT);
        await db.Metadata.Data.Apply(db);

        var lresult = await db.GetQuery("ContactType").Asc(CommonConstants.FIELD_ID).Load();
        Assert.Equal(5, lresult.ItemCount);

        IEntity ct01 = lresult.First();
        Assert.Equal("Employee", ct01["Name"]);
        Assert.Equal(1L, (long)ct01["Id"]);

        IEntity ct02 = lresult.Last();
        Assert.Equal("Corporate", ct02["Name"]);
        Assert.Equal(32L, (long)ct02["Id"]);
    }

    //Enum dynamic values


    [Fact]
    public async Task Tags_01_Basics()
    {
        var db = Database.Scopes.Db();

        //Database.Metadata.ReAdd<ConcreteEntityForTagTest>();
        db.ReAddReCreate<ConcreteEntityForTagTest>();
        db.ReAddReCreate<TagRecord>();

        IDbEntityMetadata bem = (IDbEntityMetadata)db.Metadata.Get<ConcreteEntityForTagTest>();
        bem.AddBehavior<HasTags>(true, true);
        db.Structure.ApplyEntityStructure<ConcreteEntityForTagTest>();

        //dbScope.Structure.ApplyAllStructure();
        //dbScope.Structure.DropEntity<ConcreteEntityForTagTest>();
        //dbScope.Structure.DropEntity<TagRecord>();
        //dbScope.Structure.ApplyAllStructure();

        Assert.Equal(0, await db.GetQuery<ConcreteEntityForTagTest>().Count());
        Assert.Equal(0, await db.GetQuery<TagRecord>().Count());

        ConcreteEntityForTagTest ent01 = db.New<ConcreteEntityForTagTest>();
        ent01.Name = "Entity 01";

        var tags = ent01.B<HasTags>().Tags;
        Assert.Empty(tags);

        ent01.B<HasTags>().Add("Tag 01");
        ent01.B<HasTags>().Add("Tag 02");
        ent01.B<HasTags>().Add("Tag 02");
        ent01.B<HasTags>().Add("Tag 02");
        ent01.Save();

        await db.CommitOrApplyChanges();

        Tags tags01 = ent01.GetValue<Tags>(HasTags.FIELD_TAGS);
        Assert.Equal("|Tag 01|Tag 02|", tags01.Value);

        Thread.Sleep(1000);


        var tagRecords = await db.GetQuery<TagRecord>().Load();
        Assert.Equal(2, tagRecords.ItemCount);

        TagRecord rec01 = tagRecords.First();
        Assert.Equal("ConcreteEntityForTagTest", rec01.Entity);
        Assert.Equal("Tag 01", rec01.Name);
    }

    [Fact]
    public async Task DateTime_01_UseToDateTimeOffset()
    {
        var db = Database.Scopes.Db();
        db.ReAddReCreate<ConcreteEntity01>();

        ConcreteEntity01 ent01 = db.New<ConcreteEntity01>();
        DateTimeOffset dtoRef = new DateTimeOffset(2024, 12, 01, 02, 03, 04, TimeSpan.Zero);
        //dto.Offset = TimeSpan.FromHours(3);

        ent01.BirthDate = dtoRef;
        ent01.Save();
        await db.CommitOrApplyChanges();

        long id = ent01.Id;

        ConcreteEntity01 ent01Loaded = await db.GetQuery<ConcreteEntity01>().Find(id);
        Assert.Equal(dtoRef, ent01Loaded.BirthDate);


        //Offset verilmiş olsa da UTC+0 olarak saklanmalı
        DateTimeOffset dtoWithOffset = dtoRef.ToOffset(TimeSpan.FromHours(3));

        ent01.BirthDate = dtoWithOffset;
        ent01.Save();
        await db.CommitOrApplyChanges();

        id = ent01.Id;

        ent01Loaded = await db.GetQuery<ConcreteEntity01>().Find(id);
        Assert.Equal(dtoRef, ent01Loaded.BirthDate);

        dtoWithOffset = dtoRef.ToOffset(-1 * TimeSpan.FromHours(3));

        ent01.BirthDate = dtoWithOffset;
        ent01.Save();
        await db.CommitOrApplyChanges();

        id = ent01.Id;

        ent01Loaded = await db.GetQuery<ConcreteEntity01>().Find(id);
        Assert.Equal(dtoRef, ent01Loaded.BirthDate);

        //ConcreteEntity01 ent02 = scope.New<ConcreteEntity01>();
        //ent02.BirthDate = DateTime.Now;
        //ent02.Save();


    }

    [Fact]
    public async Task Password_01_ConcreteEntity()
    {
        var dbScope = Database.Scopes.Db();

        dbScope.ReAddReCreate<PasswordTestEntity>();

        PasswordTestEntity ent01 = dbScope.New<PasswordTestEntity>();
        ent01.Name = "Entity 01";
        ent01.MyPassword = "123456";

        Password psw = ent01.MyPassword;
        ent01.Save();

        //Henüz prematüre bir entity üzerinde 
        Assert.Equal("123456", psw.Value);
        await dbScope.CommitOrApplyChanges();

        string cryptValue = psw.Value;
        Assert.NotEqual("123456", cryptValue);

        Assert.StartsWith("|C|", cryptValue);

        string decryptedValue = ent01.MyPassword.Decrypt();
        Assert.Equal("123456", decryptedValue);


    }

    [Fact]
    public async Task Password_02_JsonEntity()
    {
        var db = Database.Scopes.Db();

        string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity08.forPasswords.json");
        var ems = db.Metadata.AddJson(content);
        Assert.NotNull(ems);
        Assert.Single(ems);

        db.Structure.ApplyEntityStructure(ems.First());

        IEntity ent01 = db.New("myJsonEntity08");
        ent01["Name"] = "Entity 01";
        ent01["MyJsonEntityPassword01"] = "123456";

        Password psw = ent01["MyJsonEntityPassword01"].As<Password>();
        ent01.Save();

        //Henüz prematüre bir entity üzerinde 
        Assert.Equal("123456", psw.Value);
        await db.CommitOrApplyChanges();

        string cryptValue = psw.Value;
        Assert.NotEqual("123456", cryptValue);

        Assert.StartsWith("|C|", cryptValue);

        string decryptedValue = ((Password)ent01["MyJsonEntityPassword01"]).Decrypt();
        Assert.Equal("123456", decryptedValue);


    }


    [Fact]
    public async Task OneWayPassword_01_CryptDecrypt()
    {
        var db = Database.Scopes.Db();

        db.ReAddReCreate<PasswordTestEntity>();

        PasswordTestEntity ent01 = db.New<PasswordTestEntity>();
        ent01.Name = "Entity 01";
        ent01.MyOneWayPassword = "123456";

        OneWayPassword psw = ent01.MyOneWayPassword;
        ent01.Save();

        //Henüz prematüre bir entity üzerinde 
        Assert.Equal("123456", psw.Value);
        await db.CommitOrApplyChanges();

        string cryptValue = psw.Value;
        Assert.NotEqual("123456", cryptValue);

        Assert.StartsWith("|S|", cryptValue);

        bool isValid = ent01.MyOneWayPassword.IsEqual("11111");
        Assert.False(isValid);

        isValid = ent01.MyOneWayPassword.IsEqual("123456");
        Assert.True(isValid);
    }

    [Fact]
    public void Currency_01_AssignmentAndSimpleMath()
    {
        decimal value1 = 123.412528378234M;
        Currency cValue1 = value1;

        Assert.Equal(value1, cValue1.Value);

        cValue1 = 8.223m;
        Currency cValue2 = 1.234m;

        Currency cValue3 = cValue1 + cValue2 + 2;
        Assert.Equal(11.457m, cValue3.Value);
    }


    [Fact]
    public async Task DateTimeStartEnd_01_Simple()
    {
        var dbScope = Database.Scopes.Db();

        dbScope.Metadata.AddIfNotExist<Contact>();
        dbScope.ReAddReCreate<ConcreteEntityForUpdateTests01>();

        ConcreteEntityForUpdateTests01 entity = dbScope.New<ConcreteEntityForUpdateTests01>();

        entity.PlannedDate.Start = new DateTimeOffset(2021, 02, 03, 04, 05, 06, TimeSpan.Zero);
        entity.PlannedDate.End = new DateTimeOffset(2021, 04, 06, 08, 05, 06, TimeSpan.Zero);

        entity.Save();

        await dbScope.CommitOrApplyChanges();
    }

   
}
