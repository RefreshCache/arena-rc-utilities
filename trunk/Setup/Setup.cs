using RefreshCache.Packager;
using RefreshCache.Packager.Migrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arena.Custom.RC.Utilities.Setup
{
    public class Setup : Migration
    {
        [MigratorVersion(1, 0, 0, 1)]
        public class AddTable__cust_rc_util_ajax_task : DatabaseMigrator
        {
            public override void Upgrade(Database db)
            {
                Table tb;


                tb = new Table("cust_rc_util_ajax_task");
                tb.Columns.Add(new Column("task_id", ColumnType.Int, ColumnAttribute.PrimaryKeyIdentity));
                tb.Columns.Add(new Column("guid", ColumnType.UniqueIdentifier, ColumnAttribute.NotNull));
                tb.Columns.Add(new Column("date_created", ColumnType.DateTime, ColumnAttribute.NotNull));
                tb.Columns[tb.Columns.Count - 1].Default = "GETDATE()";
                tb.Columns.Add(new Column("status", ColumnType.Int, 80));
                tb.Columns[tb.Columns.Count - 1].Default = "0";
                tb.Columns.Add(new Column("progress", ColumnType.Numeric, ColumnAttribute.NotNull, -1, 18, 2));
                tb.Columns[tb.Columns.Count - 1].Default = "0";
                tb.Columns.Add(new Column("message", ColumnType.NVarChar, ColumnAttribute.NotNull, 2048, -1, -1));
                tb.Columns[tb.Columns.Count - 1].Default = "''";

                db.CreateTable(tb);
            }

            public override void Downgrade(Database db)
            {
                db.DropTable("cust_rc_util_ajax_task");
            }
        }

        [MigratorVersion(1, 0, 0, 2)]
        public class AddSP__cust_rc_util_sp_getAjaxTaskByID : DatabaseMigrator
        {
            public override void Upgrade(Database db)
            {
                db.ExecuteNonQuery("CREATE PROCEDURE [cust_rc_util_sp_getAjaxTaskByID]\n" +
                    "    @TaskID int\n" +
                    "AS\n" +
                    "    SELECT * FROM [cust_rc_util_ajax_task] WHERE [task_id] = @TaskID\n"
                    );
            }

            public override void Downgrade(Database db)
            {
                db.DropProcedure("cust_rc_util_sp_getAjaxTaskByID");
            }
        }

        [MigratorVersion(1, 0, 0, 3)]
        public class AddSP__cust_rc_util_sp_getAjaxTaskByGUID : DatabaseMigrator
        {
            public override void Upgrade(Database db)
            {
                db.ExecuteNonQuery("CREATE PROCEDURE [cust_rc_util_sp_getAjaxTaskByGUID]\n" +
                    "    @Guid uniqueidentifier\n" +
                    "AS\n" +
                    "    SELECT * FROM [cust_rc_util_ajax_task] WHERE [guid] = @Guid\n"
                    );
            }

            public override void Downgrade(Database db)
            {
                db.DropProcedure("cust_rc_util_sp_getAjaxTaskByGUID");
            }
        }

        [MigratorVersion(1, 0, 0, 4)]
        public class AddSP__cust_rc_util_sp_pruneAjaxTasks : DatabaseMigrator
        {
            public override void Upgrade(Database db)
            {
                db.ExecuteNonQuery("CREATE PROCEDURE [cust_rc_util_sp_pruneAjaxTasks]\n" +
                    "    @Age int\n" +
                    "AS\n" +
                    "    DELETE FROM [cust_rc_util_ajax_task] WHERE [date_created] < DATEADD(hh, -(@Age), GETDATE())\n"
                    );
            }

            public override void Downgrade(Database db)
            {
                db.DropProcedure("cust_rc_util_sp_pruneAjaxTasks");
            }
        }

        [MigratorVersion(1, 0, 0, 5)]
        public class AddSP__cust_rc_util_sp_saveAjaxTask : DatabaseMigrator
        {
            public override void Upgrade(Database db)
            {
                db.ExecuteNonQuery(
@"CREATE PROCEDURE [dbo].[cust_rc_util_sp_saveAjaxTask]
    @TaskID int,
	@Guid uniqueidentifier,
	@Status int,
	@Progress float,
	@Message nvarchar(2000),
	@ID int OUTPUT
AS
	IF NOT EXISTS(
		SELECT * FROM [cust_rc_util_ajax_task]
		WHERE [task_id] = @TaskID
		)
	BEGIN
		INSERT INTO [cust_rc_util_ajax_task]
		(
			 [guid]
			,[status]
			,[progress]
			,[message]
		)
		VALUES
		(
			 @Guid
			,@Status
			,@Progress
			,@Message
		)

		SET @ID = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		UPDATE [cust_rc_util_ajax_task] SET
			[guid] = @Guid,
			[status] = @Status,
			[progress] = @Progress,
			[message] = @Message
			WHERE [task_id] = @TaskID
		
		SET @ID = @TaskID
	END"
                    );
            }

            public override void Downgrade(Database db)
            {
                db.DropProcedure("cust_rc_util_sp_saveAjaxTask");
            }
        }
    }
}
