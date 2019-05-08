////////////////////////////////////////////////////////////////////////////////
//  
// Copyright (c) 2015 Varian Medical Systems, Inc.
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
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Globalization;

namespace FindBrachyPlans
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                using (Application app = Application.CreateApplication())
                {
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        public enum Physician
        {
            Chen = 4806689,
            Koo = 2573524,
            Peng = 1393530,
            Schmidt = 2170517,
            Hunter = 5337978
        }


        static void Execute(Application app)
        {
            int countTotal = 0;
            int countSRS = 0;
            int countPatient = 0;
            int total = app.PatientSummaries.Count();
            string format = "dddd, MMMM dd, yyyy h:mm:ss tt";
            DateTime approvalDate;
            CultureInfo provider = CultureInfo.InvariantCulture;
            string machineID = "";
            string energyTreatment = "";
            string accessory = "";
            Console.WriteLine("Patient Count/Course Count/Patient ID/Patient Name/Physician/Course/Plan/Fractions/Daily/Total/Energy/Number of Fields/Technique/Date/Sequence/Total");

            foreach (PatientSummary ps in app.PatientSummaries.OrderBy(x=>x.CreationDateTime).Where(x => (x.Id.Length == 12 && x.Id.StartsWith("1100"))))
            {
                countTotal++;
                int patientCount = 1;
                Patient p = app.OpenPatient(ps);
                foreach (Course c in p.Courses.Where(x => !(x.Id.Contains("QA") || x.Id.Contains("test"))))
                {
                    int courseCount = 1;
                    foreach (PlanSetup plan in c.PlanSetups.Where(x => x.PlanType == PlanType.ExternalBeam && x.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved))
                    {
                        if (plan.NumberOfFractions == 1)// && !plan.Id.Contains("#") && !plan.Id.Contains(":"))
                        {
                            if (p.PrimaryOncologistId.Length == 7) //4806689 chen; 2573524 koo;
                            {
                                if (plan.PlannedDosePerFraction.Dose > 900)
                                {
                                    int numberOfFields = 0;
                                    foreach (Beam b in plan.Beams)
                                    {
                                        if (!b.IsSetupField)
                                        {
                                            energyTreatment = b.EnergyModeDisplayName;
                                            numberOfFields++;
                                            if (b.Applicator != null)
                                                accessory = b.Applicator.Name;
                                            if (b.MLC != null)
                                                accessory = b.MLCPlanType.ToString();
                                        }

                                        if (b.TreatmentUnit != null)
                                        {
                                            machineID = b.TreatmentUnit.Id;
                                        }
                                    }
                                    if (machineID.Equals("ROS_LINAC_TX"))
                                    {
                                        countPatient += patientCount;
                                        countSRS += courseCount;
                                        approvalDate = DateTime.ParseExact(plan.TreatmentApprovalDate, format, provider);
                                        Console.WriteLine(countPatient + "/" + countSRS + "/" + p.Id + "/" + p.LastName + "," + p.FirstName + "/" + Enum.GetName(typeof(Physician), Convert.ToInt32(p.PrimaryOncologistId)) + "/" + c.Id.Replace("/", "_") + "/" + plan.Id.Replace("/", "_") + "/" + plan.NumberOfFractions +
                                            "/" + Math.Round(plan.PlannedDosePerFraction.Dose)+"/"+ Math.Round(plan.TotalDose.Dose)+"/"
                                            + energyTreatment + "/" + numberOfFields+ "/" + accessory +"/" + 
                                            "\"" + approvalDate.ToShortDateString() + "\"" + "/" + countTotal + "/" + total);
                                        courseCount = 0;
                                        patientCount = 0;
                                    }

                                }
                            }
                        }
                    }
                }
                app.ClosePatient();
            }
        }
    }
}
