using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex
{
    internal class MappingHelper
    {
        public static void Setup()
        {
            JsonHelper.Setup();
            YamlHelper.Setup();
        }

        public static void Start()
        {
            JsonHelper.Start();

            Type[] mapsterRegisterTypes = Common.Assembly.FindDerivedClassTypes<IRegister>();
            IEnumerable<IRegister> mapsterRegisters = mapsterRegisterTypes.Select(x => (IRegister)Activator.CreateInstance(x));
            TypeAdapterConfig.GlobalSettings.Apply(mapsterRegisters);
            TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.Flexible);

        }
    }
}
