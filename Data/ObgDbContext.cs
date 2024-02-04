using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OBGpgm.Models;

namespace OBGpgm.Data
{
    public partial class ObgDbContext : DbContext
    {
        public ObgDbContext()
        {
        }

        public ObgDbContext(DbContextOptions<ObgDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Article> Articles { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<Draft> Drafts { get; set; } = null!;
        public virtual DbSet<Member> Members { get; set; } = null!;
        public virtual DbSet<Payout> Payouts { get; set; } = null!;
        public virtual DbSet<Photo> Photos { get; set; } = null!;
        public virtual DbSet<Player> Players { get; set; } = null!;
        public virtual DbSet<Portrait> Portraits { get; set; } = null!;
        public virtual DbSet<Ptlog> Ptlogs { get; set; } = null!;
        public virtual DbSet<Schedule> Schedules { get; set; } = null!;
        public virtual DbSet<ScoreSheet> ScoreSheets { get; set; } = null!;
        public virtual DbSet<Session> Sessions { get; set; } = null!;
        public virtual DbSet<Shark> Sharks { get; set; } = null!;
        public virtual DbSet<State> States { get; set; } = null!;
        public virtual DbSet<Team> Teams { get; set; } = null!;
        public virtual DbSet<Village> Villages { get; set; } = null!;
        public virtual DbSet<VillagesStreet> VillagesStreets { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=P3NWPLSK12SQL-v02.shr.prod.phx3.secureserver.net;Initial Catalog=obgdb;User ID=obgdb;Password=1Newpw4test2day*;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Article>(entity =>
            {

                entity.HasKey(e => e.articleId);

                entity.ToTable("Article");

            });

            modelBuilder.Entity<Comment>(entity =>
            {

                entity.HasKey(e => e.commentId);

                entity.ToTable("Comment");

            });


            modelBuilder.Entity<Draft>(entity =>
            {
                entity.ToTable("Draft");

                entity.Property(e => e.DraftId).HasColumnName("DraftID");

                entity.Property(e => e.DraftPlayerId).HasColumnName("DraftPlayerID");

                entity.Property(e => e.DraftSessionId).HasColumnName("DraftSessionID");

                entity.Property(e => e.DraftTeamId).HasColumnName("DraftTeamID");

                entity.HasOne(d => d.DraftSession)
                    .WithMany(p => p.Drafts)
                    .HasForeignKey(d => d.DraftSessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Draft_Session");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.ToTable("Member");

                entity.Property(e => e.MemberId).HasColumnName("MemberID");

                entity.Property(e => e.Address1).HasMaxLength(50);

                entity.Property(e => e.Address2).HasMaxLength(50);

                entity.Property(e => e.Bday).HasColumnType("datetime");

                entity.Property(e => e.Cellphone)
                    .HasMaxLength(50)
                    .HasColumnName("cellphone");

                entity.Property(e => e.Children).HasMaxLength(50);

                entity.Property(e => e.CurrentPlayerId).HasColumnName("CurrentPlayerID");

                entity.Property(e => e.CurrentSignUpDate).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.Evaluation).HasMaxLength(255);

                entity.Property(e => e.FirstName).HasMaxLength(255);

                entity.Property(e => e.GetsPrize).HasColumnName("getsPrize");

                entity.Property(e => e.Grand).HasMaxLength(50);

                entity.Property(e => e.Great).HasMaxLength(50);

                entity.Property(e => e.Hometown).HasMaxLength(50);

                entity.Property(e => e.Job).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(255);

                entity.Property(e => e.Military).HasMaxLength(50);

                entity.Property(e => e.MovedFrom).HasMaxLength(50);

                entity.Property(e => e.PassWord).HasMaxLength(50);

                entity.Property(e => e.ShirtSize).HasMaxLength(50);

                entity.Property(e => e.Snowbird).HasColumnName("snowbird");

                entity.Property(e => e.TeamNameIfCaptain).HasMaxLength(50);

                entity.Property(e => e.Telephone).HasMaxLength(255);

                entity.Property(e => e.UserId).HasMaxLength(50);

                entity.Property(e => e.Village).HasMaxLength(50);

                entity.Property(e => e.VillageId)
                    .HasMaxLength(50)
                    .HasColumnName("VillageID");

                entity.Property(e => e.WantsNoPicture).HasColumnName("wantsNoPicture");

                entity.Property(e => e.Wife).HasMaxLength(50);

                entity.Property(e => e.YearMoved).HasMaxLength(50);

                entity.Property(e => e.YearsMilitary).HasMaxLength(50);

                entity.Property(e => e.Zip).HasMaxLength(50);

                entity.HasOne(d => d.Portrait)
                    .WithMany(p => p.Members)
                    .HasForeignKey(d => d.PortraitId)
                    .HasConstraintName("FK_Member_Portrait");
            });

            modelBuilder.Entity<Payout>(entity =>
            {
                entity.ToTable("payout");

                entity.Property(e => e.PayoutId).HasColumnName("payoutID");

                entity.Property(e => e.CaptainId).HasColumnName("captainID");

                entity.Property(e => e.Individual)
                    .HasColumnType("decimal(16, 2)")
                    .HasColumnName("individual");

                entity.Property(e => e.Player1Id).HasColumnName("player1ID");

                entity.Property(e => e.Player2Id).HasColumnName("player2ID");

                entity.Property(e => e.Player3Id).HasColumnName("player3ID");

                entity.Property(e => e.Player4Id).HasColumnName("player4ID");

                entity.Property(e => e.Players).HasColumnName("players");

                entity.Property(e => e.SessionId).HasColumnName("sessionID");

                entity.Property(e => e.TeamId).HasColumnName("teamID");

                entity.Property(e => e.TotalPayout)
                    .HasColumnType("decimal(16, 2)")
                    .HasColumnName("totalPayout");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.Payouts)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payout_Session");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.Payouts)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_payout_Team");
            });
             
            modelBuilder.Entity<Photo>(entity =>
            {
                
                entity.HasKey(e => e.id);

                entity.ToTable("Photo");

                /*
                entity.HasOne(d => d.OwnerNavigation)
                    .WithMany(p => p.Photos)
                    .HasForeignKey(d => d.Owner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Photo_Member"); 
                */
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("Player");

                entity.Property(e => e.PlayerId).HasColumnName("PlayerID");

                entity.Property(e => e.DraftId).HasColumnName("DraftID");

                entity.Property(e => e.DraftRound)
                    .HasMaxLength(1)
                    .HasColumnName("draftRound")
                    .IsFixedLength();

                entity.Property(e => e.EndWeek)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.Property(e => e.IsBeingTraded).HasColumnName("isBeingTraded");

                entity.Property(e => e.MemberId).HasColumnName("MemberID");

                entity.Property(e => e.SessionId).HasColumnName("SessionID");

                entity.Property(e => e.SkillLevel)
                    .HasMaxLength(1)
                    .IsFixedLength();

                entity.Property(e => e.StartWeek)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.Property(e => e.TeamId).HasColumnName("TeamID");


                entity.HasOne(d => d.Member)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Player_Member");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.Players)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Player_Session");

            });

            modelBuilder.Entity<Portrait>(entity =>
            {
                entity.ToTable("Portrait");

                entity.Property(e => e.LargeImage).HasColumnName("largeImage");

                entity.Property(e => e.Memberid).HasColumnName("memberid");

                entity.Property(e => e.Notes)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("notes");

                entity.Property(e => e.ThumbImage).HasColumnName("thumbImage");

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("title");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.Portraits)
                    .HasForeignKey(d => d.Memberid)
                    .HasConstraintName("FK_Portrait_Member");
            });

            modelBuilder.Entity<Ptlog>(entity =>
            {
                entity.HasKey(e => e.Ptlid);

                entity.ToTable("ptlog");

                entity.Property(e => e.Ptlid).HasColumnName("ptlid");

                entity.Property(e => e.PtlDate)
                    .HasColumnType("datetime")
                    .HasColumnName("ptlDate");

                entity.Property(e => e.Ptlmember).HasColumnName("ptlmember");

                entity.Property(e => e.Ptlplayer).HasColumnName("ptlplayer");

                entity.Property(e => e.Ptlsession).HasColumnName("ptlsession");

                entity.Property(e => e.Ptlteam).HasColumnName("ptlteam");

                entity.Property(e => e.Ptltype).HasColumnName("ptltype");

                entity.HasOne(d => d.PtlplayerNavigation)
                    .WithMany(p => p.Ptlogs)
                    .HasForeignKey(d => d.Ptlplayer)
                    .HasConstraintName("FK_ptlog_Player");

                entity.HasOne(d => d.PtlsessionNavigation)
                    .WithMany(p => p.Ptlogs)
                    .HasForeignKey(d => d.Ptlsession)
                    .HasConstraintName("FK_ptlog_Session");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedule");
            });

            modelBuilder.Entity<ScoreSheet>(entity =>
            {
                entity.HasKey(e => new { e.SsSessionId, e.SsWeek, e.SsHteam })
                    .HasName("aaaaaScoreSheet_PK")
                    .IsClustered(false);

                entity.ToTable("ScoreSheet");

                entity.Property(e => e.SsSessionId).HasColumnName("ssSessionID");

                entity.Property(e => e.SsWeek)
                    .HasColumnName("ssWeek")
                    .HasComment("Week Number");

                entity.Property(e => e.SsHteam)
                    .HasColumnName("ssHTeam")
                    .HasComment("Team Number");

                entity.Property(e => e.SsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("ssDate")
                    .HasComment("Date of match");

                entity.Property(e => e.SsDivision).HasColumnName("ssDivision");

                entity.Property(e => e.SsHpoints).HasColumnName("ssHPoints");

                entity.Property(e => e.SsVpoints).HasColumnName("ssVPoints");

                entity.Property(e => e.SsVteam)
                    .HasColumnName("ssVTeam")
                    .HasComment("Team Number");

                entity.HasOne(d => d.SsSession)
                    .WithMany(p => p.ScoreSheets)
                    .HasForeignKey(d => d.SsSessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ScoreSheet_Session");
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("Session");

                entity.Property(e => e.SessionId).HasColumnName("SessionID");

                entity.Property(e => e.CurrentWeek)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.Property(e => e.Season)
                    .HasMaxLength(1)
                    .IsFixedLength();

                entity.Property(e => e.SecondVp1).HasColumnName("SecondVP1");

                entity.Property(e => e.SecondVp2).HasColumnName("SecondVP2");

                entity.Property(e => e.SecondVp3).HasColumnName("SecondVP3");

                entity.Property(e => e.SecondVp4).HasColumnName("SecondVP4");

                entity.Property(e => e.StartDate)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Year)
                    .HasMaxLength(4)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Shark>(entity =>
            {
                entity.Property(e => e.SharkId).HasColumnName("SharkID");

                entity.Property(e => e.MemberId).HasColumnName("memberID");

                entity.Property(e => e.PlayerId).HasColumnName("playerID");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.SessionId).HasColumnName("sessionID");

                entity.Property(e => e.SharkDate)
                    .HasColumnType("datetime")
                    .HasColumnName("sharkDate");

                entity.Property(e => e.SharkType).HasColumnName("sharkType");

                entity.Property(e => e.TeamId).HasColumnName("teamID");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.Sharks)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sharks_Player");

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.Sharks)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sharks_Session");
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("State");

                entity.Property(e => e.StateAbbrev)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.StateId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("StateID");

                entity.Property(e => e.StateName)
                    .HasMaxLength(32)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Team");

                entity.Property(e => e.TeamId).HasColumnName("TeamID");

                entity.Property(e => e.IsChampion).HasColumnName("isChampion");

                entity.Property(e => e.IsRunnerUp).HasColumnName("isRunnerUp");

                entity.Property(e => e.SessionId).HasColumnName("SessionID");

                entity.Property(e => e.TeamName)
                    .HasMaxLength(50)
                    .IsFixedLength();

                entity.HasOne(d => d.Session)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Team_Session");
            });

            modelBuilder.Entity<Village>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Village");

                entity.Property(e => e.F1).HasMaxLength(255);
            });

            modelBuilder.Entity<VillagesStreet>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("VillagesStreet");

                entity.Property(e => e.County).HasMaxLength(255);

                entity.Property(e => e.District).HasColumnName("District ");

                entity.Property(e => e.Location).HasMaxLength(255);

                entity.Property(e => e.Prefix).HasMaxLength(255);

                entity.Property(e => e.StreetName)
                    .HasMaxLength(255)
                    .HasColumnName("Street Name");

                entity.Property(e => e.Type).HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
