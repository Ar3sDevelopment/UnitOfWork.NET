namespace UnitOfWork.NET.FS.NUnit

open ClassBuilder.Classes

module Mappers =     
    type FloatMapper() = 
        inherit DefaultMapper<DoubleValue, FloatValue>()
        override __.CustomMap(source, destination) = 
            destination.Value <- float(source.Value)
            destination
    
    type DoubleMapper() = 
        inherit DefaultMapper<FloatValue, DoubleValue>()
        override __.CustomMap(source, destination) = 
            destination.Value <- double(source.Value)
            destination