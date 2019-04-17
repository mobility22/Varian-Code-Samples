////////////////////////////////////////////////////////////////////////////////
// CreateOptStructures.cs
//
//  A ESAPI v15.1+ script that demonstrates optimization structure creation.
//
// Applies to:
//      Eclipse Scripting API
//          15.1.1
//          15.5
//
// Copyright (c) 2017-2018 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Forms;

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS

{
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Width= 300, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }

    public class Script
    {
        // Change these IDs to match your clinical conventions
        const string PTV_ID = "PTV";
        const string EXPANDED_PTV_ID = "PTVExp";
        const string SCRIPT_NAME = "Opt Structures Script";

        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            if (context.Patient == null || context.StructureSet == null)
            {
                System.Windows.MessageBox.Show("Please load a patient, 3D image, and structure set before running this script.", SCRIPT_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            StructureSet ss = context.StructureSet;


            // find PTV
            Structure ptv = ss.Structures.FirstOrDefault(x => x.Id == PTV_ID);
            if (ptv == null)
            {
                System.Windows.MessageBox.Show(string.Format("'{0}' not found!", PTV_ID), SCRIPT_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            context.Patient.BeginModifications();   // enable writing with this script.


            // create the empty "ptv exp" structure
            Structure ptv_exp = ss.Structures.FirstOrDefault(x => x.Id == EXPANDED_PTV_ID);
            if (ptv_exp == null)
            {
                ptv_exp = ss.AddStructure("PTV", EXPANDED_PTV_ID);
                ptv_exp.SegmentVolume = ptv.SegmentVolume;
            }


            double angle = Convert.ToDouble(Prompt.ShowDialog("Please input the direction(degree) for ptv expension", "PTV EXP"));
            double margin = Convert.ToInt16(Prompt.ShowDialog("Please input the margin(mm) for ptv expension", "PTV EXP"));

            var nPlanes = ss.Image.ZSize;
            for (int z = 0; z < nPlanes; z++)
            {
                var contoursOnImagePlane = ptv.GetContoursOnImagePlane(z);
                if (contoursOnImagePlane != null && contoursOnImagePlane.Length > 0)
                {

                    foreach (var contour in contoursOnImagePlane)
                    {
                        VVector[] contourExp = (VVector[])contour.Clone();
                        for (int n = 0; n < margin * 10; n++)
                        {
                            for (int i = 0; i < contour.Length; i++)
                            {
                                contourExp[i][0] = contour[i][0] + Math.Cos(Math.PI / 180 * angle) * 0.1 * n; //left
                                contourExp[i][1] = contour[i][1] - Math.Sin(Math.PI / 180 * angle) * 0.1 * n; //sup
                            }
                            ptv_exp.AddContourOnImagePlane(contourExp, z);
                        }

                    }
                }
            }


            string message = string.Format("{0} volume = {2:F1}\n{1} volume = {3:F1}",
                    ptv.Id, ptv_exp.Id, ptv.Volume, ptv_exp.Volume);
            System.Windows.MessageBox.Show(message);

        }
    }
}

