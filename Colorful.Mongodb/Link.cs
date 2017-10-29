using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// ͨ�������������
    /// </summary>
    public partial class Link : BaseIntId
    {
        /// <summary>
        /// �˵����
        /// </summary>
        public long MenuId
        {
            get;
            set;
        }
        /// <summary>
        /// �û����
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// ����Id
        /// </summary>
        [BindField("����", MenuFieldType.Select)]
        public long SortId
        {
            get;
            set;
        }
        /// <summary>
        /// ����
        /// </summary>
        [BindField("����")]
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// ͼƬ
        /// </summary>
        [BindField("ͼƬ", MenuFieldType.Image)]
        public string Photo
        {
            get;
            set;
        }
        /// <summary>
        /// ����
        /// </summary>
        [BindField("����", MenuFieldType.File)]
        public string File
        {
            get; set;
        }
        /// <summary>
        /// ���ӵ�ַ
        /// </summary>
        [BindField("����")]
        public string Url
        {
            get;
            set;
        }
        /// <summary>
        /// ����
        /// </summary>
        [BindField("����", MenuFieldType.SmallEditor)]
        public string Intro
        {
            get;
            set;
        }
        /// <summary>
        /// ����
        /// </summary>
        public long ByOrder
        {
            get;
            set;
        }
        /// <summary>
        /// ����
        /// </summary>
        [BindField("����", MenuFieldType.Select)]
        public string Lang
        {
            get;
            set;
        }
        /// <summary>
        /// �����ֶ�
        /// </summary>
        [BindField("�����ֶ�")]
        public string Data
        {
            get; set;
        }
        /// <summary>
        /// �����ʶ
        /// </summary>
        [BindField("��ʶ", MenuFieldType.Select)]
        public List<int> Flags { get; set; }
        /// <summary>
        /// ��ǩ
        /// </summary>
        [BindField("��ǩ", MenuFieldType.Tag)]
        public List<string> Tags { get; set; }
    }
}
