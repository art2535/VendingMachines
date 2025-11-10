-- MonitoringController.cs
-- GET api/monitoring/network-status
SELECT d.*
FROM "devices" d
LEFT JOIN "device_statuses" ds ON d."device_status_id" = ds."id"
LEFT JOIN "modems" m ON d."modem_id" = m."id"
LEFT JOIN "device_models" dm ON d."device_model_id" = dm."id"
LEFT JOIN "device_types" dt ON dm."device_type_id" = dt."id"
/*WHERE 
    (LOWER(ds."name") LIKE LOWER('%{status}%') OR '{status}' = '')
    AND (LOWER(dt."name") LIKE LOWER('%{connectionType}%') OR '{connectionType}' = '')*/
ORDER BY d."id";

SELECT 
    COUNT(*) FILTER (WHERE LOWER(ds."name") = 'Активен') AS active_count,
    COUNT(*) FILTER (WHERE LOWER(ds."name") != 'Активен') AS inactive_count
FROM "devices" d
LEFT JOIN "device_statuses" ds ON d."device_status_id" = ds."id";

SELECT d.id, d.company_id, d.created_at, d.device_model_id, d.device_status_id, d.installation_date, d.last_service_date, d.location_id, d.modem_id, d.updated_at, d0.id, d0.color_code, d0.name, m.id, m.balance, m.brand, m.created_at, m.provider, m.serial_number, d1.id, d1.description, d1.device_type_id, d1.name, d2.id, d2.description, d2.name
FROM devices AS d
LEFT JOIN device_statuses AS d0 ON d.device_status_id = d0.id
LEFT JOIN modems AS m ON d.modem_id = m.id
LEFT JOIN device_models AS d1 ON d.device_model_id = d1.id
LEFT JOIN device_types AS d2 ON d1.device_type_id = d2.id
ORDER BY d.id

SELECT SUM(p."price")
FROM "sales" s
JOIN "products" p ON s."product_id" = p."id"
WHERE s."device_id" IN (
    SELECT d."id"
    FROM "devices" d
	LEFT JOIN "device_statuses" ds ON d."device_status_id" = ds."id"
	LEFT JOIN "device_models" dm ON d."device_model_id" = dm."id"
	LEFT JOIN "device_types" dt ON dm."device_type_id" = dt."id"
    /*WHERE 
        (LOWER(ds."name") LIKE LOWER('%{status}%') OR '{status}' = '')
        AND (LOWER(dt."name") LIKE LOWER('%{connectionType}%') OR '{connectionType}' = '')*/
)

-- GET api/monitoring/summary
SELECT s.*, p."price"
FROM "sales" s
JOIN "products" p ON s."product_id" = p."id"
WHERE s."sale_date_time" IN ('2025-01-15', '2025-01-14');

-- GET api/monitoring/sales-trend?byAmount=false
SELECT s."sale_date_time" AS date,
       COUNT(*) AS value
FROM "sales" s
WHERE s."sale_date_time" BETWEEN '2025-01-15' AND '2025-05-15'
GROUP BY s."sale_date_time"
ORDER BY s."sale_date_time";

-- GET api/monitoring/sales-trend?byAmount=true
SELECT s."sale_date_time" AS date,
       SUM(p."price") AS value
FROM "sales" s
JOIN "products" p ON s."product_id" = p."id"
WHERE s."sale_date_time" BETWEEN '2025-01-15' AND '2025-05-15'
GROUP BY s."sale_date_time"
ORDER BY s."sale_date_time";

SELECT n.id AS "Id", n.device_id AS "DeviceId", n.user_id AS "UserId", n.type AS "Type", n.message AS "Message", n.priority AS "Priority", COALESCE(n.date_time::text, '') AS "DateTime", n.confirmed AS "Confirmed"
FROM notifications AS n
ORDER BY n.date_time DESC