

namespace TDFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Xml.Serialization;

    [System.Serializable]
    public class AssetBundleConfig
    {
        [XmlElement("ABList")]
        public List<ABBase> ABList { get; set; }
    }

    [System.Serializable]
    public class ABBase
    {
        [XmlAttribute("Path")]
        public string Path { get; set; }
        [XmlAttribute("Crc")]
        public uint Crc { get; set; } //Path所对应的Crc值
        [XmlAttribute("ABName")]
        public string ABName { get; set; } //包名
        [XmlAttribute("AssetName")]
        public string AssetName { get; set; } //资源名
        [XmlElement("DependABList")]
        public List<string> DependABList { get; set; } //文件所依赖的其他AB包
    }
}

