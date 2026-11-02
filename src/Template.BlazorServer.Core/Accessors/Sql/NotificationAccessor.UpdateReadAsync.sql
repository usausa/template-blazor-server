UPDATE
    Notification
SET
    ReadAt = /*@ readAt */''
WHERE
    Id = /*@ id */0
    AND UserName = /*@ userName */''
    AND ReadAt IS NULL
