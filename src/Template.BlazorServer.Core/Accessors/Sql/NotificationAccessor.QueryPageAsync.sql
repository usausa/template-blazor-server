SELECT
    *
FROM
    Notification
WHERE
    UserName = /*@ userName */''
/*% if (unreadOnly) { */
    AND ReadAt IS NULL
/*% } */
ORDER BY
    Id DESC
LIMIT /*@ limit */20 OFFSET /*@ offset */0
