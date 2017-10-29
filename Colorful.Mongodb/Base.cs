#if !MOBILE
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colorful.Models
{
    public abstract class Base
    {
        /// <summary>
        /// 修改人
        /// </summary>
        public string ModifyUser { get; set; }
        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? LastModify { get; set; }
    }

    public abstract class BaseDate : Base
    {
        /// <summary>
        /// 添加日期
        /// </summary>
        public DateTime AddDate { get; set; }
        public BaseDate()
        {
            this.AddDate = DateTime.Now;
        }
    }

    public abstract class BaseLongId : BaseDate
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id
        {
            get;
            set;
        }
    }

    public abstract class BaseIntId : BaseDate
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id
        {
            get;
            set;
        }
    }

    public abstract class BaseId : BaseDate
    {
        private string _id;

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                    _id = ObjectId.GenerateNewId().ToString();
                return _id;
            }
            set
            {
                _id = value;
            }
        }
    }
    /// <summary>
    /// 绑定字段
    /// </summary>
    public class BindField :Attribute
    {
        public string Id { get; set; }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 字段类型：MenuFieldType
        /// </summary>
        public MenuFieldType Type { get; set; }

        /// <summary>
        /// BindField
        /// </summary>
        /// <param name="name">字段名称</param>
        /// <param name="type">字段类型</param>
        public BindField(string name, MenuFieldType type = MenuFieldType.Text)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
