namespace PunchIn.Extensions
{
    using System;
    using SP = Microsoft.SharePoint.Client;

    /// <summary>
    /// Extension class for creating sharepoint fields. Helps with the creation of SPFields when creating a new SPList
    /// </summary>
    public static class SharePointExtensions
    {
        public static SP.Field Add(this SP.FieldCollection fields, SP.FieldType fieldType, String displayName, bool addToDefaultView)
        {
            return fields.AddFieldAsXml(String.Format("<Field DisplayName='{0}' Type='{1}' />", displayName, fieldType), addToDefaultView, SP.AddFieldOptions.DefaultValue);
        }

        public static SP.Field Add(this SP.FieldCollection fields, SP.FieldType fieldType, String displayName, String[] choices, bool addToDefaultView)
        {
            return fields.AddFieldAsXml(
                String.Format("<Field DisplayName='{0}' Type='{1}'><CHOICES>{2}</CHOICES></Field>",
                    displayName,
                    fieldType,
                    String.Concat(Array.ConvertAll<String, String>(choices, choice => String.Format("<CHOICE>{0}</CHOICE>", choice)))),
                addToDefaultView, SP.AddFieldOptions.DefaultValue);
        }
    }
}
