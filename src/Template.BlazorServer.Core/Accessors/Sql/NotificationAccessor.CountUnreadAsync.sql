SELECT
    COUNT(*)
FROM
    Notification
WHERE
    UserName = /*@ userName */''
    AND ReadAt IS NULL
