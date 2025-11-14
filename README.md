![Logo](./img/peereflits-logo.svg) 

# Peereflits.Shared.Databases


`Hci.Common.Databases` handles the "nitty-gritty" details of database communication by:
1. managing database connections;
1. handle transient errors;
3. logging of statements (at debug log level).

It contains no business/domain logic, and applies [Command/Query separation](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation) (but has no dependency on [Peereflits.Shared.Commanding](https://github.com/peereflits/Shared.Commanding)).
* An `(I)DatabaseQuery` handles read/search queries (`SELECT` statements) and returns the result;
* An `(I)DatabaseCommand` handles database action (`INSERT`, `UPDATE` or `DELETE` statements) and returns no result.

The diagram below shows the logical structure of this library:

<!-- Click on the diagram to view/edit it in https://mermaid.live/ -->
[![](https://mermaid.ink/img/pako:eNqdVMtuwjAQ_BXLp6ICHxAhLsAhh0q00Jsvm3hD3SZ2ZTu0iJJv78aASEpS0V7y2IxnxjOR9zw1EnnE0xycmyvYWCiEDm9sZrTG1CujY52ZvdCM3Ttvld6wFdot2uZkDh4ScNicPbs2ZkmsH8ZKoQ9njXhmETzOk4tYEJpMlPZoM0hxOg0ci09MS493cdsWU3QZsLjJ0OBvjoOUsfu_0rGm36U1WyXx8nUV9vYv08dYGuw_WU9iN1m-Ijs3MjNFAVq2OU6NOE_hF6j9gEVsDe7tV9CQmeSVhBnYTVkP3OC86kr1sUS7u02zihea6CwkOVbrJ3Rl7qvqf076uMhfx682-RqNOv4QwvbUXC_oK0noDqrxeNrHJXS7R7LSbqwfEMIlvTa-1rre4wUWVnWD-JBTaAUoSWdBaE1w_0JRCx7Ro8QMKEjBKUiCQunNaqdTHnlb4pCX77LmO54ePMogdzRFqSiAh9P5Ut8O3w7ipL8?type=png)](https://mermaid.live/edit#pako:eNqdVMtuwjAQ_BXLp6ICHxAhLsAhh0q00Jsvm3hD3SZ2ZTu0iJJv78aASEpS0V7y2IxnxjOR9zw1EnnE0xycmyvYWCiEDm9sZrTG1CujY52ZvdCM3Ttvld6wFdot2uZkDh4ScNicPbs2ZkmsH8ZKoQ9njXhmETzOk4tYEJpMlPZoM0hxOg0ci09MS493cdsWU3QZsLjJ0OBvjoOUsfu_0rGm36U1WyXx8nUV9vYv08dYGuw_WU9iN1m-Ijs3MjNFAVq2OU6NOE_hF6j9gEVsDe7tV9CQmeSVhBnYTVkP3OC86kr1sUS7u02zihea6CwkOVbrJ3Rl7qvqf076uMhfx682-RqNOv4QwvbUXC_oK0noDqrxeNrHJXS7R7LSbqwfEMIlvTa-1rre4wUWVnWD-JBTaAUoSWdBaE1w_0JRCx7Ro8QMKEjBKUiCQunNaqdTHnlb4pCX77LmO54ePMogdzRFqSiAh9P5Ut8O3w7ipL8)

## Examples

The examples below assume that the types are registered in the DI container.


### Parameterless query

``` csharp
public  interface IGetCustomers 
{ 
  Task<IEnumerable<Customer>> Execute(); 
}

internal class GetCustomers: IGetCustomers
{
  internal const string Sql = @"
  SELECT t.Id
       , t.Name
       , ... other columns
    FROM dbo.Customers t
ORDER BY t.Name
";
  
  private readonly IDatabaseQuery query;
  
  public GetCustomersQuery(IDatabaseQuery query)
      => this.query = query;
 
  public async Task<IEnumerable<Customer>> Execute() 
      => await query.Execute<Customer>(Sql);
}
```

**Note**: `IEnumerable<Customer>` is the result type of the query.

### Parameterised query

``` csharp
public interface IGetCustomer 
{
  Task<Customer> Execute(int id);
}

internal class GetCustomer: IGetCustomer
{
  internal const string Sql = @"
SELECT t.Id
     , t.Name
     , ... other columns
  FROM dbo.Customers t
 WHERE t.Id = @Id
";

  private readonly IDatabaseQuery database;

  public GetCustomerQuery(IDatabaseQuery database)
      => this.database = database;

  public async Task<Customer> Execute(int id)
  {
    if(Id <= 0) 
    { 
      throw new ArgumentOutOfRangeException($"{nameof(id)} cannot be zero or less.", nameof(id));
    }

    IEnumerable<Customer> query = await database.Execute<Customer>(Sql, new { Id = id });
    return query.SingleOrDefault();
  }
}
```

### Parameterised command

``` csharp
public class CreateCustomerParameters
{
  // All properties are parameters of the command
  public string Name { get; set; }
  ... other properties
}

public interface ICreateCustomer
{
  Task Execute(CreateCustomerParameters parameters);
}

internal class CreateCustomer: ICreateCustomer
{
  internal const string Sql = @"
INSERT INTO dbo.Customers
( Name
, ... other columns
)
VALUES
( @Name
, ... other parameters
);
";

  private readonly IDatabaseCommand database;

  public CreateCustomer(IDatabaseCommand database)
      => this.database = database;

  public async Task Execute(CreateCustomerParameters parameters)
  {
    if (parameters == null) 
    { 
      throw new ArgumentNullException(nameof(parameters)); 
    }
    // Validate all other parameters

    await database.Execute(provider.Execute(),
                           Sql,
                           new
                           {
                               parameters.Name,
                               ... other properties that map to column names
                           }
                          );
  }
}
```

<br/><br/>

### Version support

This library supports the following .NET versions:
1. .NET 8.0
1. .NET 9.0
1. .NET 10.0

---

<p align="center">
&copy; No copyright applicable<br />
&#174; "Peereflits" is my codename.
</p>

---
