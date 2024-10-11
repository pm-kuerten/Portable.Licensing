using System.Xml.Linq;

namespace Portable.Licensing;

/// <summary>
/// Represents a dictionary of license attributes.
/// </summary>
public class LicenseAttributes
{
    #region Fields
    protected readonly XName ChildName;
    protected readonly XElement XmlData;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseAttributes"/> class.
    /// </summary>
    internal LicenseAttributes(XElement? xmlData, XName childName)
    {
        XmlData = xmlData ?? new XElement("null");
        ChildName = childName;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Adds a new element with the specified key and value
    /// to the collection.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <param name="value">The value of the element.</param>
    public virtual void Add(string key, string value)
    {
        SetChildTag(key, value);
    }

    /// <summary>
    /// Adds all new element into the collection.
    /// </summary>
    /// <param name="features">The dictionary of elements.</param>
    public virtual void AddAll(IDictionary<string, string> features)
    {
        foreach (KeyValuePair<string, string> feature in features)
            Add(feature.Key, feature.Value);
    }

    /// <summary>
    /// Removes an element with the specified key
    /// from the collection.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    public virtual void Remove(string key)
    {
        XElement? element =
            XmlData.Elements(ChildName)
                .FirstOrDefault(e => e.Attribute("name") != null && e.Attribute("name")?.Value == key);

        element?.Remove();
    }

    /// <summary>
    /// Removes all elements from the collection.
    /// </summary>
    public virtual void RemoveAll()
    {
        XmlData.RemoveAll();
    }

    /// <summary>
    /// Gets the value of an element with the
    /// specified key.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <returns>The value of the element if available; otherwise null.</returns>
    public virtual string? Get(string key)
    {
        return GetChildTag(key);
    }

    /// <summary>
    /// Gets all elements.
    /// </summary>
    /// <returns>A dictionary of all elements in this collection.</returns>
    public virtual IDictionary<string, string> GetAll()
    {
        return XmlData.Elements(ChildName).ToDictionary(e => e.Attribute("name")?.Value ?? string.Empty, e => e.Value);
    }

    /// <summary>
    /// Determines whether the specified element is in
    /// this collection.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <returns>true if the collection contains this element; otherwise false.</returns>
    public virtual bool Contains(string key)
    {
        return XmlData.Elements(ChildName).Any(e => e.Attribute("name") != null && e.Attribute("name")?.Value == key);
    }

    /// <summary>
    /// Determines whether all specified elements are in
    /// this collection.
    /// </summary>
    /// <param name="keys">The list of keys of the elements.</param>
    /// <returns>true if the collection contains all specified elements; otherwise false.</returns>
    public virtual bool ContainsAll(string[] keys)
    {
        return XmlData.Elements(ChildName).All(e => e.Attribute("name") != null && keys.Contains(e.Attribute("name")?.Value));
    }

    protected virtual void SetTag(string name, string? value)
    {
        XElement? element = XmlData.Element(name);

        if (element == null)
        {
            element = new XElement(name);
            XmlData.Add(element);
        }

        if (value != null) element.Value = value;
    }

    protected virtual void SetChildTag(string name, string? value)
    {
        XElement? element =
            XmlData.Elements(ChildName)
                .FirstOrDefault(e => e.Attribute("name") != null && e.Attribute("name")?.Value == name);

        if (element == null)
        {
            element = new XElement(ChildName);
            element.Add(new XAttribute("name", name));
            XmlData.Add(element);
        }

        if (value != null) element.Value = value;
    }

    protected virtual string? GetTag(string name)
    {
        return XmlData.Element(name)?.Value;
    }

    protected virtual string? GetChildTag(string name)
    {
        XElement? element = XmlData.Elements(ChildName)
            .FirstOrDefault(e => e.Attribute("name") != null && e.Attribute("name")?.Value == name);
        return element?.Value;
    }
    #endregion
}