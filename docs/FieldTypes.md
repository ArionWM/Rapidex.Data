## Table of Contents
- [string](#string)  
- [byte](#byte)  
- [short](#short)  
- [int (int32)](#int-int32)  
- [long (int64)](#long-int64)  
- [double (float)](#double-float)  
- [decimal](#decimal)  
- [datetime](#datetime)  
- [guid](#guid)  
- [bool (boolean, yesno)](#bool-boolean-yesno)  
- [binary (byte[])](#binary-byte)  
- [currency](#currency)  
- [percentage](#percentage)  
- [phone](#phone)  
- [email](#email)  
- [color](#color)  
- [datetimeStartEnd](#datetimestartend)  
- [image](#image)  
- [reference](#reference)  
- [tags](#tags)  
- [text](#text)  
- [json](#json)  
- [vector (float[])](#vector-float)  
- [password](#password)  
- [oneWayPassword](#onewaypassword)  
- [relationOne2N](#relationone2n)  
- [relationN2N](#relationn2n)  
- [enumeration](#enumeration)  
- 
- [url](#url)  
- [date](#date)  
- [time](#time)  
- [timespan (datetimediff)](#timespan-datetimediff)  

- [xml](#xml)  

# Field Types

A **field** can represent a simple column in a database or a more complex structure, such as **one-to-many (1:N)** or **many-to-many (N:N)** relationships.

This approach:

- Simplifies the learning and implementation of **Rapidex** for developers.  
- Makes it easier for users of applications built with **Rapidex** to customize their solutions.

---

## string

A text field with a maximum length of 250 characters.

| | |
|---|---|
| JSON Type | string |
| C# Type | string |
| SQL Type | nvarchar(250) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | 250 |
| Lazy Loading | No |

---

## byte

An 8-bit integer field.

---

## short

A 16-bit integer field.

---

## int (int32)

A 32-bit integer field.

---

## long (int64)

A 64-bit integer field.

---

## double (float)

A 64-bit floating-point number field.

---

## decimal

A 128-bit decimal number field.

---

## datetime

A date and time field.

| | |
|---|---|
| JSON Type | datetime, datetimeoffset, date, time |
| C# Type | DateTimeOffset |
| SQL Type | datetime2 |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | --- |
| Lazy Loading | No |

---

## date

Date-only field.  
_Not yet implemented._

---

## time

Time-only field.  
_Not yet implemented._

---

## timespan (datetimediff)

Time span field stored as `Int64`.  
_Not yet implemented._

---

## guid

A GUID field.

---

## bool (boolean, yesno)

A logical value field.

| | |
|---|---|
| JSON Type | bool, boolean, yesno |
| C# Type | bool |
| SQL Type | bit |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | --- |
| Lazy Loading | No |

---

## binary (byte[])

A binary/blob data field.

| | |
|---|---|
| JSON Type | binary, byte[] |
| C# Type | byte[] |
| SQL Type | image |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | Unlimited |
| Lazy Loading | No |

---

## xml

_Not yet implemented._

---

## currency

See: `Currency.cs`  
A currency field. This type automatically adds an extra text field named `<FieldName>Currency` to store the ISO 4217 currency code.

| | |
|---|---|
| JSON Type | currency |
| C# Type | Currency |
| SQL Type | decimal |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | 24,8 |
| Lazy Loading | No |
| Extra Field | Yes: Adds `<FieldName>Currency` |

---

## percentage

Stores percentage values in a 0-100 range.

| | |
|---|---|
| JSON Type | percent |
| C# Type | Percent |
| SQL Type | short |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | 2 |
| Lazy Loading | No |

---

## phone

See: `Phone.cs`  

| | |
|---|---|
| JSON Type | phone |
| C# Type | Phone |
| SQL Type | nvarchar(20) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | 20 |
| Lazy Loading | No |

---

## email

See: `Email.cs`  

| | |
|---|---|
| JSON Type | email |
| C# Type | Email |
| SQL Type | nvarchar(200) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | 200 |
| Lazy Loading | No |

---

## url

_Not yet implemented._

---

## color

Stores a color value as a hex string or color name.  
Custom-defined for UI support (e.g., color picker).

| | |
|---|---|
| JSON Type | color |
| C# Type | Color |
| SQL Type | nvarchar(20) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | 20 |
| Lazy Loading | No |

---

## datetimeStartEnd

Represents a start and end date/time pair.  

| | |
|---|---|
| JSON Type | datetimeStartEnd |
| C# Type | DatetimeStartEnd |
| SQL Type | Virtual field |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | --- |
| Lazy Loading | No |
| Extra Fields | Yes: Adds `<FieldName>Start` and `<FieldName>End` |

**Note:**  
The `datetimeStartEnd` field is virtual and relies on two underlying fields: `<FieldName>Start` and `<FieldName>End`.

---

## image

A binary blob field for images.  

| | |
|---|---|
| JSON Type | image |
| C# Type | Image |
| SQL Type | image |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | Yes |

### Lazy Loading

- Value is stored in a `BlobRecord` entry and not loaded directly with the entity.
- Use `GetContent()` to retrieve the content.

Example API response:

```json
"FieldName": {
   "value": "<BlobRecordId>",
   "text": "picture",
   "id": "<schemaName>.<OwnerEntityName>.<OwnerEntityId>.<FieldName>"
}
```

---

## reference

A one-to-one reference field.

| | |
|---|---|
| C# Type | Reference<> |
| JSON Type | reference (requires target entity name) |
| SQL Type | int64 |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | Yes |

**Note:** Cross-schema usage is not yet implemented.

---

## tags

A tag field, used only with `HasTags` behavior.

| | |
|---|---|
| C# Type | Tags |
| JSON Type | tags |
| SQL Type | string |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |

---

## text

An unlimited-length text field (blob) with support for different content types (plain text, HTML, Markdown).

| | |
|---|---|
| C# Type | Text |
| JSON Type | text |
| SQL Type | nvarchar(max) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |
| Extra Field | Yes: Adds `<FieldName>Type` |

`Type` values:

| Value | Description |
|---|---|
| `null` or `text` | Plain text |
| `html` | HTML content |
| `markdown` | Markdown content |

---

## json

A text field with unlimited length. Practically, there is no difference from `Text` in the UI (for now).

| | |
|---|---|
| C# Type | Json |
| JSON Type | json |
| SQL Type | nvarchar(max) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |

---

## vector (float[])

A vector field for numerical data, commonly used for ML or embeddings.

| | |
|---|---|
| JSON Type | vector |
| C# Type | vector |
| SQL Type | vector |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Length | 1024 |
| Lazy Loading | No |

_Not yet implemented._

### YAML Definition

```yaml
vector:
  type: vector
  length: 1024
```

---

## richText

(Deprecated, will be merged into `text`.)

An unlimited-length text field (blob) for storing **rich text (HTML)** content.

| | |
|---|---|
| C# Type | RichText |
| JSON Type | richText |
| SQL Type | nvarchar(max) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |
| Extra Field | No |

---

## markdown

(Deprecated, will be merged into `text`.)

An unlimited-length text field (blob) for storing **Markdown** content.

| | |
|---|---|
| C# Type | Markdown |
| JSON Type | markdown |
| SQL Type | nvarchar(max) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |
| Extra Field | No |

---

## password

A field for **encrypted data** with two-way encryption (encrypt/decrypt).

| | |
|---|---|
| C# Type | Password |
| JSON Type | password |
| SQL Type | nvarchar(200) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |

### Details

- Encrypted information is stored in the `Value` property of `Password` objects.  
- Use `Password.Decrypt()` to decrypt values.  
- During JSON serialization, the actual password is hidden and replaced with `*****`.  
- Encryption uses the schema name and entity ID. Copying encrypted data to another schema/entity will make it **non-decryptable**.

---

## oneWayPassword

A field for **one-way encrypted** (non-reversible) passwords.  
Use `OneWayPassword.IsEqual()` to compare input values.

| | |
|---|---|
| C# Type | OneWayPassword |
| JSON Type | oneWayPassword |
| SQL Type | nvarchar(200) |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |

### Details

- Stores irreversible encrypted data.  
- JSON serialization masks the content as `*****`.  

---

## relationOne2N

Represents a **One-to-Many** relationship.

| | |
|---|---|
| C# Type | RelationOne2N<DetailEntity> |
| JSON Type | relationOne2N (requires target entity in `reference`) |
| SQL Type | int64 |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | Yes |

### Lazy Loading

- For concrete types: Implicit conversion to `DetailEntity[]`. Use `GetContent()` for lazy fetch.
- For dynamic/JSON: Use `ILazy.GetContent()`.

### Example Metadata

A `Parent<ParentEntityName>` field is added to the detail entity (e.g., `ParentInvoice`).

### Data Update

#### Add/Remove Relation

```json
{
  "UpdType": "AddRelation",
  "Relation": "<MasterEntityName>/<FieldName>",
  "MasterId": <master entity Id (can be premature)>,
  "DetailId": <detail entity Id (can be premature)>
}
```

```json
{
  "UpdType": "RemoveRelation",
  "Relation": "<MasterEntityName>/<FieldName>",
  "MasterId": <master entity Id (can't be premature)>,
  "DetailId": <detail entity Id (can't be premature)>
}
```

Example:

```json
{
  "UpdType": "AddRelation",
  "Relation": "Project/Tasks",
  "MasterId": -1000,
  "DetailId": -1001
}
```

```json
{
  "UpdType": "RemoveRelation",
  "Relation": "Project/Tasks",
  "MasterId": 1000,
  "DetailId": 1234
}
```

---

## relationN2N

Represents a **Many-to-Many** relationship.

| | |
|---|---|
| C# Type | RelationN2N<OtherEntity> |
| JSON Type | relationN2N (requires target entity in `reference`) |
| SQL Type | int64 |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | Yes |

### Lazy Loading

- For concrete types: Implicit conversion to `OtherEntity[]`. Use `GetContent()` for lazy fetch.
- For dynamic/JSON: Not yet implemented.

### Details

A linking table will be created for N2N relationships (details pending).

---

## enumeration


| | |
|---|---|
| C# Type | Enumeration<T> (T: C# Enum) |
| JSON Type | enum (requires entity name for values) |
| SQL Type | int32 |
| SQLite Type | --- |
| PostgreSQL Type | --- |
| Lazy Loading | No |

### Details

- Enum values are stored in the database as entities matching the C# enum name.  
- JSON serialization is not yet implemented.



