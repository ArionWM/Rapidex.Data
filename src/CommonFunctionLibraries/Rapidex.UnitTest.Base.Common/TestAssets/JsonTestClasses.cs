//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Rapidex.UnitTest.Base.Common.TestAssets;

//[JsonDerivedBase]
//internal interface IJsonTestBaseInterfaceA
//{
//    string Name { get; set; }
//}


//internal class JsonTestClassA_A : IJsonTestBaseInterfaceA
//{
//    public string Name { get; set; }
//}


//internal class JsonTestClassA_A_A<T> : JsonTestClassA_A
//{
//    public T Value { get; set; }
//}

//internal class JsonTestClassA_A_A_Int : JsonTestClassA_A_A<int>
//{
//    public int DValue { get; set; }
//}

//internal class JsonTestClassA_B : IJsonTestBaseInterfaceA
//{
//    public string Name { get; set; }
//}

//[JsonDerivedBase]
//internal class JsonTestClassA_B_A : JsonTestClassA_B
//{
//    public string Description { get; set; }
//}


//internal class JsonTestClassA_B_A_A<T> : JsonTestClassA_B_A
//{
//    public T Value { get; set; }
//}

//internal class JsonTestClassA_B_A_A_String : JsonTestClassA_B_A_A<string>
//{
//    public int DValue { get; set; }
//}