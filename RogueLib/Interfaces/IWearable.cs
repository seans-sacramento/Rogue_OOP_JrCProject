using System;
using System.Collections.Generic;
using System.Text;

namespace RogueLib.Interfaces;

public interface IWearable
{
    //Don armor and weapons, keep open for different item types
    void Wear();
}
