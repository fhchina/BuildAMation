﻿// <copyright file="StringArray.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class StringArray : Array<string>, System.ICloneable
    {
        public StringArray()
            : base()
        {
        }

        public StringArray(params string[] itemsToAdd)
        {
            foreach (string item in itemsToAdd)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public StringArray(System.Collections.ICollection collection)
        {
            foreach (string item in collection)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public StringArray(StringArray array)
        {
            foreach (string item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public StringArray(Opus.Core.Array<string> array)
        {
            foreach (string item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public override void Add(string item)
        {
            if (System.String.IsNullOrEmpty(item))
            {
                return;
            }

            this.list.Add(item);
        }

        public override string ToString()
        {
            return this.ToString(' ');
        }

        public string ToString(char separator)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (string item in this.list)
            {
                builder.AppendFormat("{0}{1}", item.ToString(), separator);
            }
            // remove the trailing separator
            string output = builder.ToString().TrimEnd(separator);
            return output;
        }

        public string ToString(string separator)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (string item in this.list)
            {
                builder.AppendFormat("{0}{1}", item.ToString(), separator);
            }
            // remove the trailing separator
            string output = builder.ToString().TrimEnd(separator.ToCharArray());
            return output;
        }

        public void RemoveDuplicates()
        {
            System.Collections.Generic.List<string> newList = new System.Collections.Generic.List<string>();
            foreach (string item in this.list)
            {
                if (!newList.Contains(item))
                {
                    newList.Add(item);
                }
            }

            this.list = newList;
        }

        object System.ICloneable.Clone()
        {
            StringArray clone = new StringArray();
            clone.list.AddRange(this.list);
            return clone;
        }
    }
}