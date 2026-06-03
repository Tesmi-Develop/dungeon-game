using System.Collections.Generic;
using System.Linq;

namespace DotTiled.Properties;

/// <summary>
/// Represents a class property.
/// </summary>
public class ClassProperty : HasPropertiesBase, IProperty<IList<IProperty>>
{
  /// <inheritdoc/>
  public required string Name { get; set; }

  /// <inheritdoc/>
  public PropertyType Type => Properties.PropertyType.Class;

  /// <summary>
  /// The type of the class property. This will be the name of a custom defined
  /// type in Tiled.
  /// </summary>
  public required string PropertyType { get; set; }

  public object SourceValue => Value;
  
  /// <summary>
  /// The properties of the class property.
  /// </summary>
  public required IList<IProperty> Value { get; set; }

  /// <inheritdoc/>
  public IProperty Clone() => new ClassProperty
  {
    Name = Name,
    PropertyType = PropertyType,
    Value = Value.Select(property => property.Clone()).ToList()
  };

  /// <inheritdoc/>
  public override IList<IProperty> GetProperties() => Value;
}
