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
            int total = app.PatientSummaries.Count();
            string format = "dddd, MMMM dd, yyyy h:mm:ss tt";
            DateTime approvalDate;
            CultureInfo provider = CultureInfo.InvariantCulture;

            foreach (PatientSummary ps in app.PatientSummaries.Where(x => (x.Id.Length == 12 && x.Id.StartsWith("1100"))))
            {
                //Console.WriteLine(ps.Id);

                countTotal++;
                Patient p = app.OpenPatient(ps);
                foreach (Course c in p.Courses.Where(x => !(x.Id.Contains("QA") || x.Id.Contains("test"))))
                {
                    foreach (PlanSetup plan in c.PlanSetups.Where(x => x.PlanType == PlanType.ExternalBeam && x.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved))
                    {
                        if (plan.NumberOfFractions == 1 && !plan.Id.Contains("#") && !plan.Id.Contains(":"))
                        {
                            if (p.PrimaryOncologistId.Length == 7) //4806689 chen; 2573524 koo;
                            {
                                if (plan.PlannedDosePerFraction.Dose > 900)
                                {
                                    
                                    countSRS++;
                                    approvalDate = DateTime.ParseExact(plan.TreatmentApprovalDate, format, provider);
                                    Console.WriteLine(countSRS + "/" + p.Id + "/" +Enum.GetName(typeof(Physician), Convert.ToInt32(p.PrimaryOncologistId)) + "/" + c.Id.Replace("/", "_") + "/" + plan.Id.Replace("/", "_") + "/" + plan.PlannedDosePerFraction.Dose +
                                        "/" + "\""+ approvalDate.ToShortDateString()+"\"" + "/" + countTotal + "/" + total);

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
