///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;
using Client.Network;
using Client.Sheet;

namespace Client.Brick
{
    internal class BrickSheet : EntitySheet
    {
        public BrickSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {

        }

        public override void Build(object item)
        {

        }

        public override void Serial(BitMemoryStream f)
        {
            throw new NotImplementedException();
        }

        public override void Serial(BitStreamFile f)
        {
            //throw new NotImplementedException();
        }
    }
}