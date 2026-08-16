namespace WatchListScreening.Domain.Enums;

/// <summary>
/// Category of the data source — determines what kind of data is harvested.
/// NOT to be confused with EntityType (Person/Organization).
/// PEP = Politically Exposed Person
/// </summary>
public enum SourceCategory
{
    PEP = 1,
    Individual = 2,
    Corporate = 3,
    Mixed = 4
}
