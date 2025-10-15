# Filter Syntax

Rapidex can use non concrete (text) filters for data listing. 

## Filter Syntax

The filter syntax is a string that can be used to filter data in a list. The basic syntax is as follows:

`FieldName Operator Value`

or

`(FieldName Operator Value) and/or (FieldName Operator Value)`

Parentheses are used to group conditions and can be nested, and the `and` and `or` keywords are used to combine multiple conditions. The following operators are supported:

## Simple / Direct Operators

| Operator | Description | Alternative | Status | 
| -------- | ----------- | ---------- | ------- |
| `=`        | Equals      | `eq` |  |
| `!=`       | Not equals  | `<>`, `ne`, `noteq` |  |
| `>`        | Greater than | `gt` |  |
| `<`        | Less than   | `lt` |  |
| `>=`       | Greater than or equal to | `ge`, `gte` |  |
| `<=`       | Less than or equal to | `le`, `lte` |  |
| `between`  | between in and equal to two values  |  |  |
| `:=`      | In a list (seperated by comma)  | `in`, `contains`, `anyof` |  |
| `!:`      | Not in a list | `notin`, `notcontains`, `noneof` | |
| `!`        | Not    | `not` |  |
| `~`        | Like (wildcard: `*`)      | `like` |  |
| `!~`       | Not like (wildcard: `*`)  | `notlike` | Not available |
| `is null`  | Is null     | `isnull`, `= null` | Not available |
| `is not null` | Is not null | `isnotnull`, `!= null` | Not available |


## Simple / Direct Operators
| Operator | Description | Alternative | Status | Sample | 
| -------- | ----------- | ---------- | ------- | ------- |
| `any()`    | Any record available in relation (list)  | ? | Not available |`MyEntity.RelationField.any()`|
|`Related`|**Reverse** relation with parent entity description|`relation`||`Releated = Quote/123/Items` (*Entity* a with id *123*, have *MyField* relation. Get this (my type) releated detail entities.)|

## Predefined Field Names

| Field Name | Description | Status |
| ---------- | ----------- | ------- |
| `Caption` | For entity caption field (Name, Title eg) |  
| `All` | Entity wide search | Not available |

## Predefined Values

| Value | Description | Status |
| ----- | ----------- | ----------- | 
| `true` | Boolean true | |
| `false` | Boolean false | |
| `null` | Null value | |
| `today` | Current date | |
| `now` | Current date and time ||
| `yesterday` | Yesterday's date ||
| `tomorrow` | Tomorrow's date | |
| `thisweek` | This week's start date ||
| `lastweek` | Last week's start date ||
| `nextweek` | Next week's start date ||
| `lastmonth` | Last month's start date ||
| `thismonth` | Next month's start date ||
| `nextmonth` | Next month's date ||
| `lastyear` | Last year's start date ||
| `nextyear` | Next year's start date ||

## Templates

Same filter & filter propositions have templates. See: Layouts / RelationX2N's; `filter` property

| Template | Description | Status | Sample |
| -------- | ----------- | ------- | ------- |
| `@parentId` | Releated parent id (Master / details) | Not available| `ParentQuote = @parentId` |

## Using Strings and DateTimes

Strings with spaces can be use with quotes (`'`) or without quotes (should use with url encoding).

DateTimes should be in `yyyy-MM-dd` or `yyyy-MM-ddTHH:mm:ss` format.

## Planned Features

| Feature | Description | Status |
| ------- | ----------- | ------- |
| Operator: `is null` | Is null | Not available |
| Operator: `is not null` | Is not null | Not available |
| Nested filters (MyEntity.ReferencedField.Field = ABC) | Is empty | Not available |
| Relation supported filters (MyEntity.RelationField.any() eg) ||
| string quotes; `'` ||



## Samples

### Simple Filter

```plaintext
Name = John%20Doe
```

or 

```plaintext
Name = 'John Doe'
```

```plaintext
Date between '2025-01-01' and '2025-12-31'
```

### Filter with Parentheses

```plaintext
(Name = John%20Doe) and (Age > 30)
```

```plaintext
(Name = John%20Doe) & (Age > 30)
```

### Filter with Nested Parentheses
```plaintext
((Name = John%20Doe) and (Age > 30)) or ((Name = Jane%20Doe) and (Age < 25))
```

```plaintext
((Name = John%20Doe) & (Age > 30)) | ((Name = Jane%20Doe) & (Age < 25))
```

### Using `in` Operator

`:=`, `in`, `contains`, `anyof` are same operators

```plaintext
age := 10,11,12	
```
or 

```plaintext
age in 10,11,12
```



### Filter with search any caption field

`Caption` is a special field name that can be used to search for any primary field in the entity. 
Example: *Name, Title, Subject, FullName* etc. 

See: *DbEntityMetadata.Caption* property

```plaintext
Caption~*Mytext*
```
