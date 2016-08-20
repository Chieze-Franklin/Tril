using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tril.Attributes
{
    /// <summary>
    /// Represents an extra information (metadata)
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
        AllowMultiple = true, Inherited = true)]
    public abstract class NoteAttribute : TrilAttribute
    {
        string _note = "";

        /// <summary>
        /// Creates a new instance of Tril.NoteAttribute
        /// </summary>
        public NoteAttribute(string note) : this(note, "*") { }
        /// <summary>
        /// Creates a new instance of Tril.NoteAttribute
        /// </summary>
        /// <param name="targetPlats"></param>
        public NoteAttribute(string note, params string[] targetPlats) : base(targetPlats) 
        {
            _note = note == null ? "" : note.Trim();
        }

        /// <summary>
        /// Gets the note
        /// </summary>
        public string Note
        {
            get { return _note; }
        }
    }

    /// <summary>
    /// Represents a meta data, like a C# attribute or a Java annotation.
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
        AllowMultiple = true, Inherited = false)]
    public sealed class AnnotationAttribute : NoteAttribute
    {
        /// <summary>
        /// Creates a new instance of Tril.AnnotationAttribute
        /// </summary>
        public AnnotationAttribute(string annotation) : this(annotation, "*") { }
        /// <summary>
        /// Creates a new instance of Tril.AnnotationAttribute
        /// </summary>
        /// <param name="targetPlats"></param>
        public AnnotationAttribute(string annotation, params string[] targetPlats) : base(annotation, targetPlats) { }
    }

    /// <summary>
    /// Represents a comment.
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
        AllowMultiple = true, Inherited = false)]
    public sealed class CommentAttribute : NoteAttribute
    {
        /// <summary>
        /// Creates a new instance of Tril.CommentAttribute
        /// </summary>
        public CommentAttribute(string comment) : this(comment, "*") { }
        /// <summary>
        /// Creates a new instance of Tril.CommentAttribute
        /// </summary>
        /// <param name="targetPlats"></param>
        public CommentAttribute(string comment, params string[] targetPlats) : base(comment, targetPlats) { }
    }
}
