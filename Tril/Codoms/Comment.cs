﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

using Tril.Attributes;
using Tril.Delegates;
using Tril.Models;

namespace Tril.Codoms
{
    /// <summary>
    /// Represents a comment
    /// </summary>
    [Serializable]
    public sealed class Comment : Codom
    {
        string _cmt;
        bool _isBlock = true;

        /// <summary>
        /// Creates a new instance of Tril.Codoms.Comment
        /// </summary>
        /// <param name="comment"></param>
        public Comment(string comment)
        {
            _cmt = comment == null ? "" : comment;
        }

        /// <summary>
        /// Gets the comment
        /// </summary>
        public string CommentString
        {
            get { return _cmt; }
        }
        /// <summary>
        /// Gets or sets a value to determine if the comment is to
        /// be displayed as a block comment or not (that is, as a line comment).
        /// </summary>
        public bool IsBlockComment
        {
            get { return _isBlock; }
            set { _isBlock = value; }
        }

        /// <summary>
        /// Returns the C# representation of this code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null);
        }
        /// <summary>
        /// Returns the string representation of this code, as generated by the specified translator.
        /// </summary>
        /// <param name="translator"></param>
        /// <returns></returns>
        public override string ToString(CodomTranslator translator)
        {
            string trans = Codom.ToString(translator, this);
            if (trans != null)
                return trans;
            else
            {
                if (IsBlockComment)
                {
                    string finalCommentString = "";
                    string[] commentLines = CommentString.Split('\r', '\n');
                    foreach (string line in commentLines)
                    {
                        finalCommentString += "*" + line + "\r\n";
                    }
                    finalCommentString = finalCommentString.TrimEnd('\n').TrimEnd('\r');
                    finalCommentString = finalCommentString.Replace("*/", "* /");
                    finalCommentString = "/" + finalCommentString + "*/";
                    return finalCommentString;
                }
                else
                {
                    string finalCommentString = "";
                    string[] commentLines = CommentString.Split('\r', '\n');
                    foreach (string line in commentLines)
                    {
                        finalCommentString += "//" + line + "\r\n";
                    }
                    finalCommentString = finalCommentString.TrimEnd('\n').TrimEnd('\r');
                    return finalCommentString;
                }
            }
        }
    }
}
