#!/bin/bash
# =====================================================================
# SQL Server entrypoint
# รัน SQL Server แล้วรอจน startup เสร็จก่อนรัน init.sql
# =====================================================================

# start SQL Server เป็น background process
/opt/mssql/bin/sqlservr &

# รอ SQL Server พร้อม (ลองทุก 5 วินาที, สูงสุด 60 วินาที)
for i in {1..12}; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "SQL Server is ready"
        break
    fi
    echo "Waiting for SQL Server to start... ($i/12)"
    sleep 5
done

# รัน init script
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /usr/src/app/init.sql

# keep container running
wait
