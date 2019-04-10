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

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        // Change these IDs to match your clinical conventions
        const string ChestWall_ID = "ChestWall";
        const string Lung_ID = "Lungs";
        const string MIP_ID = "#MIP";
        const string ITV_ID = "#ITV";
        const string SCRIPT_NAME = "Opt Structures Script";

        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            if (context.Patient == null || context.StructureSet == null)
            {
                MessageBox.Show("Please load a patient, 3D image, and structure set before running this script.", SCRIPT_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            StructureSet ss = context.StructureSet;
            PlanSetup ps = context.PlanSetup;
            ps.DoseValuePresentation = DoseValuePresentation.Absolute;
            Dose ds = context.PlanSetup.Dose;


            DoseValue dv = new DoseValue(3000.00000, DoseValue.DoseUnit.cGy);
            //MessageBox.Show($"{dv.Dose} {dv.Unit} {ds.DoseMax3D} {ps.DoseValuePresentation}");
            context.Patient.BeginModifications();   // enable writing with this script.
            Structure lung = ss.Structures.FirstOrDefault(x => x.Id == Lung_ID);
            Structure mip = ss.Structures.FirstOrDefault(x => x.Id == MIP_ID);
            Structure itv = ss.Structures.FirstOrDefault(x => x.Id == ITV_ID);

            Structure chestWall = ss.Structures.FirstOrDefault(x => x.Id == ChestWall_ID);
            if (chestWall == null)
            {
                chestWall = ss.AddStructure("ORGAN", "ChestWall");
            }
            else
            {
                chestWall.SegmentVolume = chestWall.SegmentVolume.Sub(chestWall.SegmentVolume);
            }

            chestWall.ConvertDoseLevelToStructure(ds, dv);
            if (lung != null)
            {
                chestWall.SegmentVolume = chestWall.SegmentVolume.Sub(lung.SegmentVolume);
            }
            if (mip != null)
            {
                chestWall.SegmentVolume = chestWall.SegmentVolume.Sub(mip.SegmentVolume);
            }
            if (itv != null)
            {
                chestWall.SegmentVolume = chestWall.SegmentVolume.Sub(itv.SegmentVolume);
            }
            MessageBox.Show($"V chestwall @30Gy = {chestWall.Volume:F1} cc");

        }
    }
}

