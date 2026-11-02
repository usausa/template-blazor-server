DELETE FROM
    Notification
WHERE
    Id IN (
        SELECT
            Id
        FROM
            Notification
        WHERE
            CreatedAt < /*@ before */''
        ORDER BY
            Id
        LIMIT /*@ limit */1000
    )
