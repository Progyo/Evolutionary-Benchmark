using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public interface IMutatable : IComponentData
{
    public void Mutate(object a);
}