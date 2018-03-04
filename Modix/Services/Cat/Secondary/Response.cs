namespace Modix.Services.Cat.Secondary
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class response
    {
        private responseData dataField;

        public responseData data
        {
            get => dataField;
            set => dataField = value;
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class responseData
    {
        private responseDataImage[] imagesField;
        
        [XmlArrayItem("image", IsNullable = false)]
        public responseDataImage[] images
        {
            get => imagesField;
            set => imagesField = value;
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class responseDataImage
    {
        private string urlField;
        private string idField;
        private string source_urlField;

       public string url
        {
            get => urlField;
            set => urlField = value;
        }
        
        public string id
        {
            get => idField;
            set => idField = value;
        }
        
        public string source_url
        {
            get => source_urlField;
            set => source_urlField = value;
        }
    }
}