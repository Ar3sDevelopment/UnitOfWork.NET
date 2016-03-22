namespace UnitOfWork.NET.NUnit

open ClassBuilder.Classes

module Mappers =     
    type FloatMapper() = 
        inherit DefaultMapper<double, float>()
        override __.CustomMap(source, destination) = 
            double(source)
    
    type DoubleMapper() = 
        inherit DefaultMapper<float, double>()
        override __.CustomMap(source, destination) = 
            double(source)