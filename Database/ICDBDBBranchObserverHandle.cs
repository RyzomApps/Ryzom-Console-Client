///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

namespace RCC.Database
{
    public abstract class ICDBDBBranchObserverHandle
    {
        ~ICDBDBBranchObserverHandle() { }

        public virtual ICDBNode owner() { return null; }
        //public virtual IPropertyObserver observer() { }
        public virtual bool observesLeaf(string leafName) { return false; }
        public virtual bool inList(uint list) { return false; }
        public virtual void addToFlushableList() { }
        public virtual void removeFromFlushableList(uint list) { }
        public virtual void removeFromFlushableList() { }

    }
}
