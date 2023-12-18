#!/bin/bash

curl https://gitlab.com/ryzom/ryzom-core/-/raw/main/atys-live/ryzom/client/data/gamedev/interfaces_v3/local_database.xml -o ./Client/Resources/local_database.xml

curl https://gitlab.com/ryzom/ryzom-core/-/raw/main/atys-live/ryzom/common/data_common/msg.xml -o ./Client/Resources/msg.xml
curl https://gitlab.com/ryzom/ryzom-core/-/raw/main/atys-live/ryzom/common/data_common/database.xml -o ./Client/Resources/database.xml

# curl https://gitlab.com/ryzom/ryzom-data/-/raw/main/atys-live/final_bnps/leveldesign/sheet_id.bin -o ./Client/Resources/sheet_id.bin

read -r -p "$*"