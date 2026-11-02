PRAGMA journal_mode = WAL;

CREATE TABLE IF NOT EXISTS Account (
    Id         INTEGER  NOT NULL,
    Name       TEXT     NOT NULL,
    Password   BLOB     NOT NULL,
    Role       TEXT     NOT NULL,
    CreatedAt  TEXT     NOT NULL,
    PRIMARY KEY (Id AUTOINCREMENT),
    UNIQUE (Name)
);

CREATE TABLE IF NOT EXISTS Data (
    Id         INTEGER  NOT NULL,
    Name       TEXT     NOT NULL,
    Value      INTEGER  NOT NULL,
    Version    INTEGER  NOT NULL,
    CreatedAt  TEXT     NOT NULL,
    PRIMARY KEY (Id AUTOINCREMENT),
    UNIQUE (Name)
);

CREATE TABLE IF NOT EXISTS Notification (
    Id         INTEGER  NOT NULL,
    UserName   TEXT     NOT NULL,
    Title      TEXT     NOT NULL,
    Body       TEXT     NOT NULL,
    Link       TEXT,
    CreatedAt  TEXT     NOT NULL,
    ReadAt     TEXT,
    PRIMARY KEY (Id AUTOINCREMENT),
    FOREIGN KEY (UserName) REFERENCES Account (Name) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Notification_UserName ON Notification (UserName, Id);
CREATE INDEX IF NOT EXISTS IX_Notification_Unread ON Notification (UserName) WHERE ReadAt IS NULL;
CREATE INDEX IF NOT EXISTS IX_Notification_CreatedAt ON Notification (CreatedAt);
