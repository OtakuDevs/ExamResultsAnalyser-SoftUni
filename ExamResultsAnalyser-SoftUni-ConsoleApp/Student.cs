using System.Collections.Generic;

namespace ExamResultsAnalyser_SoftUni_ConsoleApp
{
    public class Student
    {
        public Student()
        {
            Grades = new Dictionary<string, double>();
        }
        private string Name { get; set; }

        private Dictionary<string,double> Grades { get; set; }// <- To get the values 
        
    }
}