using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JsonSocialNetwork.MigrationEF.Migrations
{
    public partial class JSNDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    phone = table.Column<string>(type: "varchar(10)", nullable: false),
                    password = table.Column<string>(type: "varchar(10)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(31)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime", nullable: false),
                    description = table.Column<string>(type: "nvarchar(127)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(127)", nullable: true),
                    is_admin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.UniqueConstraint("AK_accounts_phone", x => x.phone);
                });

            migrationBuilder.CreateTable(
                name: "contents",
                columns: table => new
                {
                    file_name = table.Column<string>(type: "varchar(31)", nullable: false),
                    content_type = table.Column<string>(type: "varchar(31)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contents", x => x.file_name);
                });

            migrationBuilder.CreateTable(
                name: "blocks",
                columns: table => new
                {
                    blocker_account_id = table.Column<int>(type: "int", nullable: false),
                    blocked_account_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocks", x => new { x.blocker_account_id, x.blocked_account_id });
                    table.ForeignKey(
                        name: "FK_blocks_accounts_blocked_account_id",
                        column: x => x.blocked_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_blocks_accounts_blocker_account_id",
                        column: x => x.blocker_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    owner_account_id = table.Column<int>(type: "int", nullable: false),
                    partner_account_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                    table.UniqueConstraint("AK_conversations_owner_account_id_partner_account_id", x => new { x.owner_account_id, x.partner_account_id });
                    table.ForeignKey(
                        name: "FK_conversations_accounts_owner_account_id",
                        column: x => x.owner_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversations_accounts_partner_account_id",
                        column: x => x.partner_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "friend_requests",
                columns: table => new
                {
                    sender_account_id = table.Column<int>(type: "int", nullable: false),
                    receiver_account_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friend_requests", x => new { x.sender_account_id, x.receiver_account_id });
                    table.ForeignKey(
                        name: "FK_friend_requests_accounts_receiver_account_id",
                        column: x => x.receiver_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_friend_requests_accounts_sender_account_id",
                        column: x => x.sender_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "friends",
                columns: table => new
                {
                    smaller_account_id = table.Column<int>(type: "int", nullable: false),
                    bigger_account_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friends", x => new { x.smaller_account_id, x.bigger_account_id });
                    table.ForeignKey(
                        name: "FK_friends_accounts_bigger_account_id",
                        column: x => x.bigger_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_friends_accounts_smaller_account_id",
                        column: x => x.smaller_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime", nullable: false),
                    date_modified = table.Column<DateTime>(type: "datetime", nullable: false),
                    author_account_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_posts_accounts_author_account_id",
                        column: x => x.author_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "avatars",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "int", nullable: false),
                    content_file_name = table.Column<string>(type: "varchar(31)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avatars", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_avatars_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_avatars_contents_content_file_name",
                        column: x => x.content_file_name,
                        principalTable: "contents",
                        principalColumn: "file_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    author_account_id = table.Column<int>(type: "int", nullable: false),
                    conversation_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_accounts_author_account_id",
                        column: x => x.author_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime", nullable: false),
                    author_account_id = table.Column<int>(type: "int", nullable: false),
                    owner_post_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_comments_accounts_author_account_id",
                        column: x => x.author_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_comments_posts_owner_post_id",
                        column: x => x.owner_post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "likes",
                columns: table => new
                {
                    author_account_id = table.Column<int>(type: "int", nullable: false),
                    owner_post_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_likes", x => new { x.author_account_id, x.owner_post_id });
                    table.ForeignKey(
                        name: "FK_likes_accounts_author_account_id",
                        column: x => x.author_account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_likes_posts_owner_post_id",
                        column: x => x.owner_post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_contents",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false),
                    post_id = table.Column<int>(type: "int", nullable: false),
                    content_file_name = table.Column<string>(type: "varchar(31)", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_post_contents_contents_content_file_name",
                        column: x => x.content_file_name,
                        principalTable: "contents",
                        principalColumn: "file_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_post_contents_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subject = table.Column<byte>(type: "tinyint", nullable: false),
                    detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    post_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_reports_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "contents",
                columns: new[] { "file_name", "content_type" },
                values: new object[] { "default_avatar.png", "image/png" });

            migrationBuilder.InsertData(
                table: "contents",
                columns: new[] { "file_name", "content_type" },
                values: new object[] { "404_favicon.png", "image/png" });

            migrationBuilder.InsertData(
                table: "contents",
                columns: new[] { "file_name", "content_type" },
                values: new object[] { "404_page.jpg", "image/jpeg" });

            migrationBuilder.CreateIndex(
                name: "IX_avatars_content_file_name",
                table: "avatars",
                column: "content_file_name");

            migrationBuilder.CreateIndex(
                name: "IX_blocks_blocked_account_id",
                table: "blocks",
                column: "blocked_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_author_account_id",
                table: "comments",
                column: "author_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_owner_post_id",
                table: "comments",
                column: "owner_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_partner_account_id",
                table: "conversations",
                column: "partner_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_friend_requests_receiver_account_id",
                table: "friend_requests",
                column: "receiver_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_friends_bigger_account_id",
                table: "friends",
                column: "bigger_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_likes_owner_post_id",
                table: "likes",
                column: "owner_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_author_account_id",
                table: "messages",
                column: "author_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_conversation_id",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_contents_content_file_name",
                table: "post_contents",
                column: "content_file_name");

            migrationBuilder.CreateIndex(
                name: "IX_post_contents_post_id",
                table: "post_contents",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_author_account_id",
                table: "posts",
                column: "author_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_post_id",
                table: "reports",
                column: "post_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avatars");

            migrationBuilder.DropTable(
                name: "blocks");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "friend_requests");

            migrationBuilder.DropTable(
                name: "friends");

            migrationBuilder.DropTable(
                name: "likes");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "post_contents");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "contents");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
