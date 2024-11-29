REM gamedev
curl https://gitlab.com/ryzom/ryzom-core/-/raw/main/atys-live/ryzom/client/data/gamedev/interfaces_v3/local_database.xml > .\Client\Resources\local_database.xml

REM data_common
curl https://gitlab.com/ryzom/ryzom-core/-/raw/main/atys-live/ryzom/common/data_common/msg.xml > .\Client\Resources\msg.xml
curl https://gitlab.com/ryzom/ryzom-core/-/raw/main/atys-live/ryzom/common/data_common/database.xml > .\Client\Resources\database.xml

REM leveldesign
REM curl https://gitlab.com/ryzom/ryzom-data/-/raw/main/atys-live/final_bnps/leveldesign/sheet_id.bin > .\Client\Resources\sheet_id.bin

pause
