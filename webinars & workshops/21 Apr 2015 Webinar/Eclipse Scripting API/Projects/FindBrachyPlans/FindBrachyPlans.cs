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
    static void Execute(Application app)
    {
      foreach(PatientSummary ps in app.PatientSummaries) 
      {
          Patient p = app.OpenPatient(ps);
          foreach (Course c in p.Courses)
          {
              foreach (PlanSetup plan in c.PlanSetups)
              {
                  if (plan.PlanType == PlanType.Brachy)
                  {
                      Console.WriteLine("found brachy plan = " + p.Id + "/" + c.Id + "/" + plan.Id);
                  }
              }
          }
          app.ClosePatient();
      }
    }
  }
}
