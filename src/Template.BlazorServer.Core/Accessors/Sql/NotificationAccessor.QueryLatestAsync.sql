SELECT
    *
FROM
    Notification
WHERE
    UserName = /*@ userName */''
ORDER BY
    Id DESC
LIMIT /*@ limit */5
