-- ============================================================
-- task9_ai — MySQL Schema
-- Compatible: MySQL 5.7+ / 8.0+
-- Note: CURRENT_TIMESTAMP = server timezone.
--       Set MySQL timezone to UTC for consistent UTC storage:
--       SET GLOBAL time_zone = '+00:00';
-- ============================================================

CREATE DATABASE IF NOT EXISTS task9_ai
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE task9_ai;

-- ------------------------------------------------------------
-- user_ai_conversations
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_ai_conversations (
    id                BIGINT          NOT NULL AUTO_INCREMENT,
    conversation_id   VARCHAR(64)     NOT NULL,
    user_id           BIGINT          NOT NULL,
    title             VARCHAR(500)    NOT NULL DEFAULT '',
    model             VARCHAR(100)    NOT NULL DEFAULT 'claude-sonnet-4.5',
    total_messages    INT             NOT NULL DEFAULT 0,
    last_message_at   DATETIME        NULL,
    is_pinned         TINYINT(1)      NOT NULL DEFAULT 0,
    is_deleted        TINYINT(1)      NOT NULL DEFAULT 0,
    created_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_conversation_id (conversation_id),
    INDEX idx_user_id (user_id),
    INDEX idx_user_deleted_pinned (user_id, is_deleted, is_pinned, last_message_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- user_ai_messages
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_ai_messages (
    id                BIGINT          NOT NULL AUTO_INCREMENT,
    conversation_id   VARCHAR(64)     NOT NULL,
    role              VARCHAR(20)     NOT NULL,
    content           MEDIUMTEXT      NOT NULL,
    tokens            INT             NULL,
    metadata          JSON            NULL,
    sequence          INT             NOT NULL DEFAULT 0,
    created_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    INDEX idx_conv_seq (conversation_id, sequence),
    CONSTRAINT fk_msg_conversation
        FOREIGN KEY (conversation_id)
        REFERENCES user_ai_conversations (conversation_id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- user_ai_sessions
-- One active session per user (user_id is PK)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_ai_sessions (
    user_id                 BIGINT UNSIGNED NOT NULL,
    session_id_hash         VARCHAR(64)     NOT NULL,
    session_id_encrypted    VARCHAR(256)    NOT NULL,
    conversation_id         VARCHAR(64)     NULL,
    workspace_path          VARCHAR(500)    NOT NULL DEFAULT '',
    created_at              DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_active             DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id),
    INDEX idx_session_hash (session_id_hash),
    INDEX idx_last_active (last_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- user_ai_preferences
-- One row per user
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_ai_preferences (
    user_id           BIGINT          NOT NULL,
    default_model     VARCHAR(100)    NOT NULL DEFAULT 'claude-sonnet-4.5',
    context_window    INT             NOT NULL DEFAULT 200000,
    temperature       DECIMAL(4,2)    NOT NULL DEFAULT 1.00,
    auto_save         TINYINT(1)      NOT NULL DEFAULT 1,
    show_tokens       TINYINT(1)      NOT NULL DEFAULT 0,
    theme             VARCHAR(10)     NOT NULL DEFAULT 'auto',
    language          VARCHAR(10)     NOT NULL DEFAULT 'vi-VN',
    created_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- user_ai_files
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_ai_files (
    id                BIGINT          NOT NULL AUTO_INCREMENT,
    user_id           BIGINT          NOT NULL,
    file_path         VARCHAR(500)    NOT NULL,
    file_name         VARCHAR(255)    NOT NULL,
    file_size         BIGINT          NOT NULL DEFAULT 0,
    file_hash         VARCHAR(64)     NULL,
    conversation_id   VARCHAR(64)     NULL,
    is_deleted        TINYINT(1)      NOT NULL DEFAULT 0,
    synced_at         DATETIME        NULL,
    created_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    INDEX idx_user_id (user_id),
    INDEX idx_conv_id (conversation_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- user_ai_skills
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_ai_skills (
    id                BIGINT          NOT NULL AUTO_INCREMENT,
    user_id           BIGINT          NOT NULL,
    skill_name        VARCHAR(100)    NOT NULL,
    is_enabled        TINYINT(1)      NOT NULL DEFAULT 1,
    granted_by        BIGINT          NULL,
    granted_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    revoked_at        DATETIME        NULL,
    created_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_user_skill (user_id, skill_name),
    INDEX idx_user_id (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ------------------------------------------------------------
-- user_ai_skill_audit_logs
-- Immutable audit trail
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS user_ai_skill_audit_logs (
    id                BIGINT          NOT NULL AUTO_INCREMENT,
    user_id           BIGINT          NOT NULL,
    skill_name        VARCHAR(100)    NOT NULL,
    action            VARCHAR(20)     NOT NULL,
    performed_by      BIGINT          NULL,
    reason            TEXT            NULL,
    created_at        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    INDEX idx_user_id (user_id),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
