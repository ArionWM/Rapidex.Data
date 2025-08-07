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
        var dbScope = Database.Scopes.Db();

        dbScope.ReAddReCreate<ConcreteEntity01>();


        byte[] imageContentOriginal01 = this.Fixture.GetFileContentAsBinary("TestContent\\Image01.png");
        int hash01 = HashHelper.GetStableHashCode(imageContentOriginal01);

        ConcreteEntity01 entity = dbScope.New<ConcreteEntity01>();
        entity.Name = "Binary 001";

        entity.Picture.Set(imageContentOriginal01, "image.png", MimeTypeMap.GetMimeType("png"));
        entity.Save();

        await dbScope.CommitOrApplyChanges();

        long blobId01_check = entity.Picture.TargetId;

        Database.Metadata.ReAdd<ConcreteEntity01>();

        ConcreteEntity01 ent01 = await dbScope.GetQuery<ConcreteEntity01>().First();

        long blobId01 = ent01.Picture.TargetId;

        byte[] imageContent01 = ent01.Picture;
        Assert.NotNull(imageContent01);

        int hash02_check = HashHelper.GetStableHashCode(imageContent01);
        Assert.Equal(hash01, hash02_check);


        byte[] imageContentOriginal02 = this.Fixture.GetFileContentAsBinary("TestContent\\Image02.png");
        int hash02 = HashHelper.GetStableHashCode(imageContentOriginal02);

        entity.Picture.Set(imageContentOriginal02, "image.png", MimeTypeMap.GetMimeType("png"));
        entity.Save();

        await dbScope.CommitOrApplyChanges();

        //Yeni bir içerik yüklendiğinde Id değişmemeli, önceki blobRecord kaydı güncellenmeli
        long blobId02_check = ent01.Picture.TargetId;
        Assert.Equal(blobId01_check, blobId02_check);
    }

    [Fact]
    public async Task Image_02_NullContentDontCreateOrDeleteBlobRecord()
    {
        var dbScope = Database.Scopes.Db();
        //Database.Scopes.Db()

        dbScope.ReAddReCreate<BlobRecord>();
        dbScope.ReAddReCreate<ConcreteEntity01>();

        byte[] imageContentOriginal01 = this.Fixture.GetFileContentAsBinary("TestContent\\Image01.png");
        int hash01 = HashHelper.GetStableHashCode(imageContentOriginal01);

        ConcreteEntity01 entity = dbScope.New<ConcreteEntity01>();
        entity.Name = "Binary 001";
        entity.Picture.Set(imageContentOriginal01, "image.png", MimeTypeMap.GetMimeType("png"));
        entity.Save();

        await dbScope.CommitOrApplyChanges();

        long entityId01 = entity.Id;

        long blobRecId01 = entity.Picture.TargetId;
        BlobRecord br = await dbScope.Find<BlobRecord>(blobRecId01);
        Assert.NotNull(br);

        int hash01_check = HashHelper.GetStableHashCode(br.Data);
        Assert.Equal(hash01, hash01_check);

        entity = await dbScope.Find<ConcreteEntity01>(entityId01);
        entity.Picture.SetEmpty();
        entity.Save();

        await dbScope.CommitOrApplyChanges();

        Assert.Equal(DatabaseConstants.DEFAULT_EMPTY_ID, entity.Picture.TargetId);

        br = await dbScope.Find<BlobRecord>(blobRecId01);
        Assert.Null(br);



    }

    [Fact]
    public virtual async Task Enum_01_FromJsonDefinition()
    {
        //HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        //Database.Metadata.Clear();
        //Database.Metadata.Setup(builder.Services);

        Database.Metadata.Remove("Rank");

        string content = this.Fixture.GetFileContentAsString("TestContent\\EnumDefinition.Sample01.json");
        Database.Metadata.AddFromJson(content);


        var em = Database.Metadata.Get("Rank");
        Assert.NotNull(em);

        //.IsSupportTo<Enumeration>()
        Assert.True(em.Fields.ContainsKey(CommonConstants.FIELD_ID));
        Assert.True(em.Fields.ContainsKey(CommonConstants.FIELD_EXTERNAL_ID));
        Assert.True(em.Fields.ContainsKey(CommonConstants.FIELD_VERSION));

        var dbScope = Database.Scopes.Db();

        dbScope.Structure.ApplyEntityStructure(em);

        var enumValues = await dbScope.Load(em);

        //WARN: Artık ApplyEntityStructure içerisinde _predefinedValueProcessor.Apply(this.DbScope, mdef.Data, @override); uygulanmıyor
        //TODO: Enum değerlerin yeni altyapı ile uygulanıp uygulanmadığını kontrol et

        //Assert.Equal(3, enumValues.ItemCount);

        //IEntity entity01 = enumValues.First();
        //Assert.Equal(1L, entity01[CommonConstants.FIELD_ID]);
        //Assert.Equal("Gold", entity01["Name"]);
        //Assert.True(entity01["color"].IsSupportTo<Rapidex.Data.Color>());
        //Assert.Equal("#FFD700", ((Rapidex.Data.Color)entity01["color"]).Value);


    }

    [Fact]
    public virtual async Task Enum_02_Assignment()
    {
        var scope = Database.Scopes.Db();

        scope.ReAddReCreate<ConcreteEntity01>();

        ConcreteEntity01 ent01 = scope.New<ConcreteEntity01>();
        ent01.ContactType = ContactType.Corporate;
        ent01.Save();



        ConcreteEntity01 ent02 = scope.New<ConcreteEntity01>();
        ent02["ContactType"] = ContactType.Corporate; //Bu durumda farklı bir atama yapısı çalışıyor
        ent02.Save();
        await scope.CommitOrApplyChanges();

        long id01 = ent01.Id;
        long id02 = ent02.Id;

        //this.Fixture.ClearCaches();

        //TODO: Clear entity cache

        scope = Database.Scopes.Db();
        Database.Metadata.ReAdd<ConcreteEntity01>();

        ConcreteEntity01 ent01_check = await scope.Find<ConcreteEntity01>(id01);
        Assert.Equal(ContactType.Corporate, (ContactType)ent01_check.ContactType);

        ConcreteEntity01 ent02_check = await scope.Find<ConcreteEntity01>(id02);
        Assert.Equal(ContactType.Corporate, (ContactType)ent02_check.ContactType);

    }

    [Fact]
    public virtual async Task Enum_03_PredefinedValues()
    {
        var dbScope = Database.Scopes.Db();

        dbScope.ReAddReCreate<ConcreteEntity01>();
        //Database.Metadata.AddIfNotExist<BlobRecord>();
        Database.Metadata.Remove("ContactType");

        var emCT = Database.Metadata.AddFromEnum<ContactType>();
        dbScope.Structure.DropEntity("ContactType");
        dbScope.Structure.ApplyEntityStructure(emCT);
        await Database.PredefinedValues.Apply(dbScope);

        var lresult = await dbScope.GetQuery("ContactType").Asc(CommonConstants.FIELD_ID).Load();
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
        var dbScope = Database.Scopes.Db();

        //Database.Metadata.ReAdd<ConcreteEntityForTagTest>();
        dbScope.ReAddReCreate<ConcreteEntityForTagTest>();
        dbScope.ReAddReCreate<TagRecord>();

        IDbEntityMetadata bem = (IDbEntityMetadata)Database.Metadata.Get<ConcreteEntityForTagTest>();
        bem.AddBehavior<HasTags>(true, true);
        dbScope.Structure.ApplyEntityStructure<ConcreteEntityForTagTest>();

        //dbScope.Structure.ApplyAllStructure();
        //dbScope.Structure.DropEntity<ConcreteEntityForTagTest>();
        //dbScope.Structure.DropEntity<TagRecord>();
        //dbScope.Structure.ApplyAllStructure();

        Assert.Equal(0, await dbScope.GetQuery<ConcreteEntityForTagTest>().Count());
        Assert.Equal(0, await dbScope.GetQuery<TagRecord>().Count());

        ConcreteEntityForTagTest ent01 = dbScope.New<ConcreteEntityForTagTest>();
        ent01.Name = "Entity 01";

        var tags = ent01.B<HasTags>().Tags;
        Assert.Empty(tags);

        ent01.B<HasTags>().Add("Tag 01");
        ent01.B<HasTags>().Add("Tag 02");
        ent01.B<HasTags>().Add("Tag 02");
        ent01.B<HasTags>().Add("Tag 02");
        ent01.Save();

        await dbScope.CommitOrApplyChanges();

        Tags tags01 = ent01.GetValue<Tags>(HasTags.FIELD_TAGS);
        Assert.Equal("|Tag 01|Tag 02|", tags01.Value);

        Thread.Sleep(1000);


        var tagRecords = await dbScope.GetQuery<TagRecord>().Load();
        Assert.Equal(2, tagRecords.ItemCount);

        TagRecord rec01 = tagRecords.First();
        Assert.Equal("ConcreteEntityForTagTest", rec01.Entity);
        Assert.Equal("Tag 01", rec01.Name);
    }

    [Fact]
    public async Task DateTime_01_UseToDateTimeOffset()
    {
        var scope = Database.Scopes.Db();
        scope.ReAddReCreate<ConcreteEntity01>();

        ConcreteEntity01 ent01 = scope.New<ConcreteEntity01>();
        DateTimeOffset dtoRef = new DateTimeOffset(2024, 12, 01, 02, 03, 04, TimeSpan.Zero);
        //dto.Offset = TimeSpan.FromHours(3);

        ent01.BirthDate = dtoRef;
        ent01.Save();
        await scope.CommitOrApplyChanges();

        long id = ent01.Id;

        ConcreteEntity01 ent01Loaded = await scope.GetQuery<ConcreteEntity01>().Find(id);
        Assert.Equal(dtoRef, ent01Loaded.BirthDate);


        //Offset verilmiş olsa da UTC+0 olarak saklanmalı
        DateTimeOffset dtoWithOffset = dtoRef.ToOffset(TimeSpan.FromHours(3));

        ent01.BirthDate = dtoWithOffset;
        ent01.Save();
        await scope.CommitOrApplyChanges();

        id = ent01.Id;

        ent01Loaded = await scope.GetQuery<ConcreteEntity01>().Find(id);
        Assert.Equal(dtoRef, ent01Loaded.BirthDate);

        dtoWithOffset = dtoRef.ToOffset(-1 * TimeSpan.FromHours(3));

        ent01.BirthDate = dtoWithOffset;
        ent01.Save();
        await scope.CommitOrApplyChanges();

        id = ent01.Id;

        ent01Loaded = await scope.GetQuery<ConcreteEntity01>().Find(id);
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
        var dbScope = Database.Scopes.Db();

        string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity08.forPasswords.json");
        var em = Database.Metadata.AddFromJson(content);

        dbScope.Structure.ApplyEntityStructure(em);

        IEntity ent01 = dbScope.New("myJsonEntity08");
        ent01["Name"] = "Entity 01";
        ent01["MyJsonEntityPassword01"] = "123456";

        Password psw = ent01["MyJsonEntityPassword01"].As<Password>();
        ent01.Save();

        //Henüz prematüre bir entity üzerinde 
        Assert.Equal("123456", psw.Value);
        await dbScope.CommitOrApplyChanges();

        string cryptValue = psw.Value;
        Assert.NotEqual("123456", cryptValue);

        Assert.StartsWith("|C|", cryptValue);

        string decryptedValue = ((Password)ent01["MyJsonEntityPassword01"]).Decrypt();
        Assert.Equal("123456", decryptedValue);


    }


    [Fact]
    public async Task OneWayPassword_01_CryptDecrypt()
    {
        var dbScope = Database.Scopes.Db();

        dbScope.ReAddReCreate<PasswordTestEntity>();

        PasswordTestEntity ent01 = dbScope.New<PasswordTestEntity>();
        ent01.Name = "Entity 01";
        ent01.MyOneWayPassword = "123456";

        OneWayPassword psw = ent01.MyOneWayPassword;
        ent01.Save();

        //Henüz prematüre bir entity üzerinde 
        Assert.Equal("123456", psw.Value);
        await dbScope.CommitOrApplyChanges();

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

        dbScope.ReAddReCreate<ConcreteEntityForUpdateTests01>();

        ConcreteEntityForUpdateTests01 entity = dbScope.New<ConcreteEntityForUpdateTests01>();

        entity.PlannedDate.Start = new DateTimeOffset(2021, 02, 03, 04, 05, 06, TimeSpan.Zero);
        entity.PlannedDate.End = new DateTimeOffset(2021, 04, 06, 08, 05, 06, TimeSpan.Zero);

        entity.Save();

        await dbScope.CommitOrApplyChanges();
    }

   
}
