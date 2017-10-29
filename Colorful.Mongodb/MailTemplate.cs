using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// �ʼ�ģ��
    /// </summary>
    public partial class MailTemplate : BaseIntId
    {
        /// <summary>
        /// �˵�Id
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// �ʼ�����
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// �ʼ���ǩ
        /// </summary>
        public string Label { get; set; }
		/// <summary>
        /// ����
		/// </summary>
		public string Title
		{
			get;
			set;
		}
		/// <summary>
        /// ����
		/// </summary>
		public string Content
		{
			get;
			set;
		}
		/// <summary>
		/// ����
		/// </summary>
        public string Lang
		{
			get;
			set;
		}
        /// <summary>
        /// ����
        /// </summary>
		public int ByOrder
		{
			get;
			set;
		}
	}
}
