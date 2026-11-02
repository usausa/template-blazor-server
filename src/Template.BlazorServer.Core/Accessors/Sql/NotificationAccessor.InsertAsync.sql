INSERT INTO
    Notification (UserName, Title, Body, Link, CreatedAt)
SELECT
    Name, /*@ title */'', /*@ body */'', /*@ link */'', /*@ createdAt */''
FROM
    Account
/*% if (userName != null) { */
WHERE
    Name = /*@ userName */''
/*% } */
ORDER BY
    Id
