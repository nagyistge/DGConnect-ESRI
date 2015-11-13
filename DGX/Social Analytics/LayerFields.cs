// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LayerFields.cs" company="DigitalGlobe">
//   Copyright 2015 DigitalGlobe
//   
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//   
//          http://www.apache.org/licenses/LICENSE-2.0
//   
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>

namespace Dgx
{
    using System;

    using ESRI.ArcGIS.Geodatabase;

    /// <summary>
    /// The layer fields.
    /// </summary>
    public class LayerFields
    {
        /// <summary>
        /// The create rss fields.
        /// </summary>
        /// <returns>
        /// The <see cref="IFields"/>.
        /// </returns>
        public static IFields CreateRssFields()
        {
            string doubleText =
                @"X;Y;titleNegative;descriptionPositive;luceneScore;negativeSentiment;positiveSentiment;descriptionNegative;titlePositive";
            string intText = string.Empty;
            string textText = "url";
            string shortText = "countryCode";

            IObjectClassDescription objectClassDescription = new ObjectClassDescriptionClass();
            IFields fields = null;

            // create the fields using the required fields method
            fields = objectClassDescription.RequiredFields;
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            string[] stringSeparators = new[] { ";" };
            string[] doubleTextStr = doubleText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < doubleTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = doubleTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                fieldEdit.Length_2 = 20;
                fieldsEdit.AddField(field);
            }

            if (intText != string.Empty)
            {
                string[] intTextStr = intText.Split(stringSeparators, StringSplitOptions.None);
                for (int i = 0; i < intTextStr.Length; i++)
                {
                    IField field = new FieldClass();
                    IFieldEdit fieldEdit = (IFieldEdit)field;
                    fieldEdit.Name_2 = intTextStr[i];
                    fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                    fieldEdit.Length_2 = 20;
                    fieldsEdit.AddField(field);
                }
            }

            string[] textTextStr = textText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < textTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = textTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                fieldEdit.Length_2 = 256;
                fieldsEdit.AddField(field);
            }

            string[] shortTextStr = shortText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < shortTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = shortTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                fieldEdit.Length_2 = 25;
                fieldsEdit.AddField(field);
            }

            fields = (IFields)fieldsEdit; // Explicit Cast
            return fields;
        }

        /// <summary>
        /// The create fields.
        /// </summary>
        /// <returns>
        /// The <see cref="IFields"/>.
        /// </returns>
        public static IFields CreateFields()
        {
            string doubleText = "X;Y;pos_sentiment;neg_sentiment";
            string intText = "fav_count;followers;friends;status_count";
            string textText = "id;text";
            string shortText = "cntry_code;time;display_name;geotype;verb;device;actorID";
            string hundredText = "scrn_name;actor_desc";

            IObjectClassDescription objectClassDescription = new ObjectClassDescriptionClass();
            IFields fields = null;

            // create the fields using the required fields method
            fields = objectClassDescription.RequiredFields;
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            string[] stringSeparators = new[] { ";" };
            string[] doubleTextStr = doubleText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < doubleTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = doubleTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                fieldEdit.Length_2 = 20;
                fieldsEdit.AddField(field);
            }

            string[] intTextStr = intText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < intTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = intTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                fieldEdit.Length_2 = 20;
                fieldsEdit.AddField(field);
            }

            string[] textTextStr = textText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < textTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = textTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                fieldEdit.Length_2 = 256;
                fieldsEdit.AddField(field);
            }

            string[] shortTextStr = shortText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < shortTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = shortTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                fieldEdit.Length_2 = 25;
                fieldsEdit.AddField(field);
            }

            string[] hundredTextStr = hundredText.Split(stringSeparators, StringSplitOptions.None);
            for (int i = 0; i < hundredTextStr.Length; i++)
            {
                IField field = new FieldClass();
                IFieldEdit fieldEdit = (IFieldEdit)field;
                fieldEdit.Name_2 = hundredTextStr[i];
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                fieldEdit.Length_2 = 100;
                fieldsEdit.AddField(field);
            }

            fields = (IFields)fieldsEdit; // Explicit Cast
            return fields;
        }
    }
}
