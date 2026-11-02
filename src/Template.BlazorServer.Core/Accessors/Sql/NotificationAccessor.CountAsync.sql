SELECT
    COUNT(*)
FROM
    Notification
WHERE
    UserName = /*@ userName */''
/*% if (unreadOnly) { */
    AND ReadAt IS NULL
/*% } */
